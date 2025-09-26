using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AssemblyChain.Core.DomainModel;
using AssemblyChain.Planning;

namespace AssemblyChain.Robotics;

/// <summary>
/// Minimal URScript exporter that converts plan steps into move commands.
/// </summary>
public sealed class UrScriptExporter
{
    public string Export(AssemblyPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var builder = new StringBuilder();
        builder.AppendLine("def assembly_plan():");
        foreach (var step in plan.Steps)
        {
            builder.AppendLine($"  textmsg(\"Step {step.Index}: {step.Action} {step.PartId}\")");
            if (step.Pose is { } pose)
            {
                builder.AppendLine($"  movej(p[{pose.Origin.X:F3},{pose.Origin.Y:F3},{pose.Origin.Z:F3},{pose.ZAxis.X:F3},{pose.ZAxis.Y:F3},{pose.ZAxis.Z:F3}])");
            }
        }

        builder.AppendLine("end");
        return builder.ToString();
    }
}

/// <summary>
/// Lightweight wrapper that mimics the RTDE API used to communicate with a UR10 robot.
/// </summary>
public sealed class Ur10RtdeClient
{
    private readonly List<string> _sentPrograms = new();

    public Task ConnectAsync(string host, int port)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        if (port <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        return Task.CompletedTask;
    }

    public Task SendProgramAsync(string program)
    {
        ArgumentNullException.ThrowIfNull(program);
        _sentPrograms.Add(program);
        return Task.CompletedTask;
    }

    public IReadOnlyList<string> SentPrograms => _sentPrograms;
}

/// <summary>
/// Minimal Modbus TCP wrapper for the Schunk EGH gripper.
/// </summary>
public sealed class SchunkEghClient
{
    private readonly List<string> _commands = new();

    public Task ConnectAsync(string host, int port = 502)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        if (port <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        return Task.CompletedTask;
    }

    public Task SendCommandAsync(string command)
    {
        ArgumentException.ThrowIfNullOrEmpty(command);
        _commands.Add(command);
        return Task.CompletedTask;
    }

    public IReadOnlyList<string> Commands => _commands;
}

/// <summary>
/// Dummy executor used for tests and CI. The executor plays back plan steps and records them for verification.
/// </summary>
public sealed class DummyRobotExecutor
{
    private readonly List<PlanStep> _executedSteps = new();

    public Task ExecuteAsync(AssemblyPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        _executedSteps.Clear();
        _executedSteps.AddRange(plan.Steps);
        return Task.CompletedTask;
    }

    public IReadOnlyList<PlanStep> ExecutedSteps => _executedSteps;
}
