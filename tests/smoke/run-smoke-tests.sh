#!/usr/bin/env bash
# Smoke tests for a built dotnet-sail container image.
#
# Usage: run-smoke-tests.sh <image-ref> <flavor>
#   <image-ref>  A local or registry image reference that can be passed to `docker run`.
#   <flavor>     One of: net10, net8, net9, all. Selects the expected SDK/runtime contract.
#
# These checks validate image *contents* (SDK/runtime set and DOTNET_ROLL_FORWARD),
# gating whether a per-architecture build is eligible to be included in the final
# manifest. They run conventional and native file-based source fixtures through
# Sail, plus framework-dependent runtime probes, to verify execution and roll-forward.
set -euo pipefail

if [ "$#" -ne 2 ]; then
	echo "Usage: $0 <image-ref> <flavor>" >&2
	exit 2
fi

IMAGE="$1"
FLAVOR="$2"
SCRIPT_DIRECTORY="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
FIXTURES_DIRECTORY="${SCRIPT_DIRECTORY}/fixtures"
TEMPORARY_DIRECTORY="$(mktemp -d)"
SERVE_DIRECTORY="${TEMPORARY_DIRECTORY}/serve"
PROBES_DIRECTORY="${TEMPORARY_DIRECTORY}/probes"
NETWORK_NAME="sail-smoke-${RANDOM}-${RANDOM}"
SERVER_NAME="sail-smoke-server-${RANDOM}-${RANDOM}"

fail() {
	echo "SMOKE TEST FAILED (${IMAGE}, flavor=${FLAVOR}): $1" >&2
	exit 1
}

cleanup() {
	docker rm --force "${SERVER_NAME}" >/dev/null 2>&1 || true
	docker network rm "${NETWORK_NAME}" >/dev/null 2>&1 || true
	rm -rf "${TEMPORARY_DIRECTORY}"
}

trap cleanup EXIT

assert_output_contains() {
	local output="$1" expected="$2"
	echo "${output}" | grep -Fqx "${expected}" \
		|| fail "application output did not contain '${expected}'"
}

assert_framework_major() {
	local output="$1" major="$2"
	echo "${output}" | grep -Eq "^SMOKE_FRAMEWORK=\.NET ${major}\." \
		|| fail "application did not use .NET ${major}.x runtime"
}

prepare_fixture_server() {
	mkdir -p "${SERVE_DIRECTORY}"
	cp "${FIXTURES_DIRECTORY}/file-based/App.cs" "${SERVE_DIRECTORY}/file-based.cs"

	(
		cd "${FIXTURES_DIRECTORY}/conventional"
		python3 -m zipfile -c "${SERVE_DIRECTORY}/conventional.zip" Conventional.csproj Program.cs
	)

	docker network create "${NETWORK_NAME}" >/dev/null
	docker run --detach --name "${SERVER_NAME}" --network "${NETWORK_NAME}" \
		--volume "${SERVE_DIRECTORY}:/www:ro" busybox:1.37.0 \
		httpd -f -p 8080 -h /www >/dev/null

	for _ in $(seq 1 30); do
		if docker run --rm --network "${NETWORK_NAME}" busybox:1.37.0 \
			wget -q -O /dev/null "http://${SERVER_NAME}:8080/file-based.cs"; then
			return
		fi
		sleep 1
	done

	fail "fixture HTTP server did not become reachable"
}

run_sail_fixture() {
	local fixture="$1"
	docker run --rm --network "${NETWORK_NAME}" "${IMAGE}" \
		"http://${SERVER_NAME}:8080/${fixture}"
}

echo "== Smoke testing '${IMAGE}' (flavor: ${FLAVOR}) =="

# --- 1. dotnet --list-sdks contains .NET 10 SDK and no .NET 8/9 SDK. ---
sdks="$(docker run --rm --entrypoint dotnet "${IMAGE}" --list-sdks)"
echo "--- dotnet --list-sdks ---"
echo "${sdks}"

echo "${sdks}" | grep -Eq '^10\.' || fail "no .NET 10 SDK found"
if echo "${sdks}" | grep -Eq '^(8|9)\.'; then
	fail ".NET 8/9 SDK unexpectedly present (images must contain only the .NET 10 SDK)"
fi

# --- 2. dotnet --list-runtimes exactly matches the flavor contract. ---
runtimes="$(docker run --rm --entrypoint dotnet "${IMAGE}" --list-runtimes)"
echo "--- dotnet --list-runtimes ---"
echo "${runtimes}"

assert_runtime_major() {
	local framework="$1" major="$2"
	echo "${runtimes}" | grep -Eq "^${framework} ${major}\." \
		|| fail "expected ${framework} ${major}.x runtime to be present"
}

assert_no_runtime_major() {
	local framework="$1" major="$2"
	if echo "${runtimes}" | grep -Eq "^${framework} ${major}\."; then
		fail "${framework} ${major}.x runtime unexpectedly present"
	fi
}

case "${FLAVOR}" in
net10)
	assert_runtime_major Microsoft.NETCore.App 10
	assert_runtime_major Microsoft.AspNetCore.App 10
	assert_no_runtime_major Microsoft.NETCore.App 8
	assert_no_runtime_major Microsoft.AspNetCore.App 8
	assert_no_runtime_major Microsoft.NETCore.App 9
	assert_no_runtime_major Microsoft.AspNetCore.App 9
	;;
net8)
	assert_runtime_major Microsoft.NETCore.App 10
	assert_runtime_major Microsoft.AspNetCore.App 10
	assert_runtime_major Microsoft.NETCore.App 8
	assert_runtime_major Microsoft.AspNetCore.App 8
	assert_no_runtime_major Microsoft.NETCore.App 9
	assert_no_runtime_major Microsoft.AspNetCore.App 9
	;;
net9)
	assert_runtime_major Microsoft.NETCore.App 10
	assert_runtime_major Microsoft.AspNetCore.App 10
	assert_runtime_major Microsoft.NETCore.App 9
	assert_runtime_major Microsoft.AspNetCore.App 9
	assert_no_runtime_major Microsoft.NETCore.App 8
	assert_no_runtime_major Microsoft.AspNetCore.App 8
	;;
all)
	assert_runtime_major Microsoft.NETCore.App 10
	assert_runtime_major Microsoft.AspNetCore.App 10
	assert_runtime_major Microsoft.NETCore.App 9
	assert_runtime_major Microsoft.AspNetCore.App 9
	assert_runtime_major Microsoft.NETCore.App 8
	assert_runtime_major Microsoft.AspNetCore.App 8
	;;
*)
	fail "unknown flavor '${FLAVOR}' (expected one of: net10, net8, net9, all)"
	;;
esac

# --- 3. DOTNET_ROLL_FORWARD is 'Major'. ---
roll_forward="$(docker run --rm --entrypoint /bin/sh "${IMAGE}" -c 'printf "%s" "${DOTNET_ROLL_FORWARD:-}"')"
echo "--- DOTNET_ROLL_FORWARD ---"
echo "${roll_forward}"
[ "${roll_forward}" = "Major" ] || fail "DOTNET_ROLL_FORWARD is '${roll_forward}', expected 'Major'"

prepare_fixture_server

# --- 4. A conventional project builds and runs. ---
conventional_output="$(run_sail_fixture conventional.zip)" \
	|| fail "conventional project did not build and run"
echo "--- conventional project output ---"
echo "${conventional_output}"
assert_output_contains "${conventional_output}" "SMOKE_CONVENTIONAL"
assert_framework_major "${conventional_output}" 10

# --- 5. A native file-based application builds and runs. ---
file_based_output="$(run_sail_fixture file-based.cs)" \
	|| fail "native file-based application did not build and run"
echo "--- native file-based application output ---"
echo "${file_based_output}"
assert_output_contains "${file_based_output}" "SMOKE_FILE_BASED"
assert_framework_major "${file_based_output}" 10

# --- 6. Target-framework-specific framework-dependent apps use the expected runtime. ---
run_target_framework_probe() {
	local target_major="$1" runtime_major="$2"
	local marker="SMOKE_FRAMEWORK_DEPENDENT_NET${target_major}"
	local output_directory="${PROBES_DIRECTORY}/net${target_major}"
	local output

	mkdir -p "${output_directory}"
	docker run --rm --entrypoint dotnet \
		--volume "${FIXTURES_DIRECTORY}/framework-dependent/net${target_major}:/src:ro" \
		--volume "${output_directory}:/out" \
		--workdir /src \
		"mcr.microsoft.com/dotnet/sdk:${target_major}.0-noble" \
		build --configuration Release --output /out \
		|| fail "net${target_major}.0 runtime probe did not build"

	output="$(docker run --rm --entrypoint dotnet \
		--volume "${output_directory}:/probe:ro" \
		"${IMAGE}" /probe/RuntimeProbe.dll)" \
		|| fail "net${target_major}.0 runtime probe did not run"
	echo "--- net${target_major}.0 runtime probe output ---"
	echo "${output}"
	assert_output_contains "${output}" "${marker}"
	assert_framework_major "${output}" "${runtime_major}"
}

case "${FLAVOR}" in
net10)
	run_target_framework_probe 8 10
	run_target_framework_probe 9 10
	;;
net8)
	run_target_framework_probe 8 8
	;;
net9)
	run_target_framework_probe 9 9
	;;
all)
	run_target_framework_probe 8 8
	run_target_framework_probe 9 9
	;;
esac

echo "== Smoke tests passed for '${IMAGE}' (flavor: ${FLAVOR}) =="
