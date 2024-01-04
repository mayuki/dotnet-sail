namespace Sail.Tests;

public class SailRunOptionsTest
{
    [Fact]
    public void UpdateFromCommandLineArgument_Empty()
    {
        // Arrange
        string[] args = [];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.Equal(SailRunOptions.Default.Arguments, options.Arguments);
        Assert.Equal(SailRunOptions.Default.Configuration, options.Configuration);
        Assert.Equal(SailRunOptions.Default.EnvironmentVariables, options.EnvironmentVariables);
        Assert.Equal(SailRunOptions.Default.ExecName, options.ExecName);
        Assert.Equal(SailRunOptions.Default.LaunchProfile, options.LaunchProfile);
        Assert.Equal(SailRunOptions.Default.Runner, options.Runner);
        Assert.Equal(SailRunOptions.Default.Sdk, options.Sdk);
        Assert.Equal(SailRunOptions.Default.Source, options.Source);
        Assert.Equal(SailRunOptions.Default.Verbosity, options.Verbosity);
    }

    [Fact]
    public void UpdateFromCommandLineArgument_Complex()
    {
        // Arrange
        string[] args = ["-e", "Foo=Bar", "-r", "publish", "http://www.example.com/", "arg1", "arg2", "-r", "-c", "-e", "arg3"];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal("http://www.example.com/", options.Source);
        Assert.Equal(["arg1", "arg2", "-r", "-c", "-e", "arg3"], options.Arguments);
        Assert.Equal(SailRunOptions.Default.Configuration, options.Configuration);
        Assert.Contains("Foo", options.EnvironmentVariables);
        Assert.Equal(SailRunOptions.Default.ExecName, options.ExecName);
        Assert.Equal(SailRunOptions.Default.LaunchProfile, options.LaunchProfile);
        Assert.Equal("publish", options.Runner);
        Assert.Equal(SailRunOptions.Default.Sdk, options.Sdk);
        Assert.Equal(SailRunOptions.Default.Verbosity, options.Verbosity);
    }

    [Fact]
    public void UpdateFromCommandLineArgument_Source_Preserve()
    {
        // Arrange
        string[] args = [];
        var options = SailRunOptions.Default with { Source = "https://www.example.com/" };

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal("https://www.example.com/", options.Source);
        Assert.Equal(SailRunOptions.Default.Arguments, options.Arguments);
        Assert.Equal(SailRunOptions.Default.Configuration, options.Configuration);
        Assert.Equal(SailRunOptions.Default.EnvironmentVariables, options.EnvironmentVariables);
        Assert.Equal(SailRunOptions.Default.ExecName, options.ExecName);
        Assert.Equal(SailRunOptions.Default.LaunchProfile, options.LaunchProfile);
        Assert.Equal(SailRunOptions.Default.Runner, options.Runner);
        Assert.Equal(SailRunOptions.Default.Sdk, options.Sdk);
        Assert.Equal(SailRunOptions.Default.Verbosity, options.Verbosity);
    }

    [Fact]
    public void UpdateFromCommandLineArgument_Source_Only()
    {
        // Arrange
        string[] args = ["http://www.example.com/"];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal("http://www.example.com/", options.Source);
        Assert.Equal(SailRunOptions.Default.Arguments, options.Arguments);
        Assert.Equal(SailRunOptions.Default.Configuration, options.Configuration);
        Assert.Equal(SailRunOptions.Default.EnvironmentVariables, options.EnvironmentVariables);
        Assert.Equal(SailRunOptions.Default.ExecName, options.ExecName);
        Assert.Equal(SailRunOptions.Default.LaunchProfile, options.LaunchProfile);
        Assert.Equal(SailRunOptions.Default.Runner, options.Runner);
        Assert.Equal(SailRunOptions.Default.Sdk, options.Sdk);
        Assert.Equal(SailRunOptions.Default.Verbosity, options.Verbosity);
    }

    [Fact]
    public void UpdateFromCommandLineArgument_Source_With_Argument()
    {
        // Arrange
        string[] args = ["http://www.example.com/", "arg1", "arg2", "-r", "-c", "-e", "arg3"];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal("http://www.example.com/", options.Source);
        Assert.Equal(["arg1", "arg2", "-r", "-c", "-e", "arg3"], options.Arguments);
        Assert.Equal(SailRunOptions.Default.Configuration, options.Configuration);
        Assert.Equal(SailRunOptions.Default.EnvironmentVariables, options.EnvironmentVariables);
        Assert.Equal(SailRunOptions.Default.ExecName, options.ExecName);
        Assert.Equal(SailRunOptions.Default.LaunchProfile, options.LaunchProfile);
        Assert.Equal(SailRunOptions.Default.Runner, options.Runner);
        Assert.Equal(SailRunOptions.Default.Sdk, options.Sdk);
        Assert.Equal(SailRunOptions.Default.Verbosity, options.Verbosity);
    }

    [Fact]
    public void UpdateFromCommandLineArgument_Options_Only()
    {
        // Arrange
        string[] args = ["-r", "runner_test", "-c", "DebugConfig"];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(SailRunOptions.Default.Source, options.Source);
        Assert.Equal(SailRunOptions.Default.Arguments, options.Arguments);
        Assert.Equal("DebugConfig", options.Configuration);
        Assert.Equal(SailRunOptions.Default.EnvironmentVariables, options.EnvironmentVariables);
        Assert.Equal(SailRunOptions.Default.ExecName, options.ExecName);
        Assert.Equal(SailRunOptions.Default.LaunchProfile, options.LaunchProfile);
        Assert.Equal("runner_test", options.Runner);
        Assert.Equal(SailRunOptions.Default.Sdk, options.Sdk);
        Assert.Equal(SailRunOptions.Default.Verbosity, options.Verbosity);
    }

    [Theory]
    [InlineData("-r", "runner_test")]
    [InlineData("--runner", "runner_test")]
    public void UpdateFromCommandLineArgument_Options_Runner(string optionName, string optionValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.Runner);
    }

    [Theory]
    [InlineData("-e", "Foo=Bar", "Foo", "Bar")]
    [InlineData("-e", "Foo==B=A=R=", "Foo", "=B=A=R=")]
    [InlineData("--env", "Hoge==F=u=g=a=", "Hoge", "=F=u=g=a=")]
    public void UpdateFromCommandLineArgument_Options_EnvironmentVariables(string optionName, string optionValue, string envName, string envValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Contains(options.EnvironmentVariables, kv => kv.Key == envName && kv.Value == envValue);
    }

    [Fact]
    public void UpdateFromCommandLineArgument_Options_EnvironmentVariables_Multiple()
    {
        // Arrange
        string[] args = ["-e", "Foo=Bar", "-e", "Hoge=Fuga", "--env", "Alice=Karen"];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Contains("Foo", options.EnvironmentVariables);
        Assert.Equal("Bar", options.EnvironmentVariables["Foo"]);
        Assert.Contains("Hoge", options.EnvironmentVariables);
        Assert.Equal("Fuga", options.EnvironmentVariables["Hoge"]);
        Assert.Contains("Alice", options.EnvironmentVariables);
        Assert.Equal("Karen", options.EnvironmentVariables["Alice"]);
    }

    [Fact]
    public void UpdateFromCommandLineArgument_Options_EnvironmentVariables_Preserve()
    {
        // Arrange
        string[] args = ["-e", "Foo=Bar"];
        var options = SailRunOptions.Default;

        // Act
        options = options with { EnvironmentVariables = new Dictionary<string, string>() { { "Key1", "Value2" }, { "Foo", "FromEnvVar" } } };
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Contains("Key1", options.EnvironmentVariables);
        Assert.Equal("Value2", options.EnvironmentVariables["Key1"]);
        Assert.Contains("Foo", options.EnvironmentVariables);
        Assert.Equal("Bar", options.EnvironmentVariables["Foo"]);
    }

    [Theory]
    [InlineData("-s", "https://www.example.com/1.git")]
    [InlineData("--source", "https://www.example.com/2.git")]
    public void UpdateFromCommandLineArgument_Options_Source(string optionName, string optionValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.Source);
    }

    [Theory]
    [InlineData("-c", "__Debug__")]
    [InlineData("--configuration", "__Debug__")]
    public void UpdateFromCommandLineArgument_Options_Configuration(string optionName, string optionValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.Configuration);
    }

    [Theory]
    [InlineData("-v", "Error", LogLevel.Error)]
    [InlineData("--verbosity", "t", LogLevel.Trace)]
    public void UpdateFromCommandLineArgument_Options_Verbosity(string optionName, string optionValue, LogLevel logLevel)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(logLevel, options.Verbosity);
    }

    [Theory]
    [InlineData("--exec-name", "MyApp")]
    public void UpdateFromCommandLineArgument_Options_ExecName(string optionName, string optionValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.ExecName);
    }

    [Theory]
    [InlineData("--sdk", "____.NET.Sdk")]
    public void UpdateFromCommandLineArgument_Options_Sdk(string optionName, string optionValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.Sdk);
    }

    [Theory]
    [InlineData("--target-framework", "net7.0-windows")]
    public void UpdateFromCommandLineArgument_Options_TargetFramework(string optionName, string optionValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.TargetFramework);
    }

    [Theory]
    [InlineData("--launch-profile", "http")]
    public void UpdateFromCommandLineArgument_Options_LaunchProfile(string optionName, string optionValue)
    {
        // Arrange
        string[] args = [optionName, optionValue];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.LaunchProfile);
    }

    [Theory]
    [InlineData("--no-launch-profile", true)]
    public void UpdateFromCommandLineArgument_Options_NoLaunchProfile(string optionName, bool optionValue)
    {
        // Arrange
        string[] args = [optionName];
        var options = SailRunOptions.Default;

        // Act
        options = SailRunOptions.UpdateFromCommandLineArguments(options, args);

        // Assert
        Assert.NotEqual(SailRunOptions.Default, options);
        Assert.Equal(optionValue, options.NoLaunchProfile);
    }
}