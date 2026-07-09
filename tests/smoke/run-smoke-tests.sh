#!/usr/bin/env bash
# Smoke tests for a built dotnet-sail container image.
#
# Usage: run-smoke-tests.sh <image-ref> <flavor>
#   <image-ref>  A local or registry image reference that can be passed to `docker run`.
#   <flavor>     One of: net10, net8, net9, all. Selects the expected SDK/runtime contract.
#
# These checks validate image *contents* (SDK/runtime set and DOTNET_ROLL_FORWARD),
# gating whether a per-architecture build is eligible to be included in the final
# manifest. Application-execution smoke tests (conventional project, native
# file-based application, target-framework roll-forward probes) are a follow-up
# handoff item and are intentionally out of scope here.
set -euo pipefail

if [ "$#" -ne 2 ]; then
	echo "Usage: $0 <image-ref> <flavor>" >&2
	exit 2
fi

IMAGE="$1"
FLAVOR="$2"

fail() {
	echo "SMOKE TEST FAILED (${IMAGE}, flavor=${FLAVOR}): $1" >&2
	exit 1
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

echo "== Smoke tests passed for '${IMAGE}' (flavor: ${FLAVOR}) =="
