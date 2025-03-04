﻿using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitHub;
using Octokit;

namespace MaaBuilder;

public partial class Build
{
    Target UseCommitVersion => _ => _
        .Triggers(SetVersion)
        .Executes(() =>
        {
            Version = $"{Parameters.BuildTime}-{Parameters.CommitHash}";
        });

    Target UseTagVersion => _ => _
        .Triggers(SetVersion)
        .Executes(() =>
        {
            if (Parameters.IsGitHubActions is false)
            {
                Information("未在 GitHub Actions 中运行");

                var repo = GitRepository.FromLocalDirectory(RootDirectory);
                Assert.True(repo is not null, "不在 Git Repo 中");
                
                var tag = repo.Tags.FirstOrDefault();

                if (tag is not null)
                {
                    Version = $"{tag}-Local";
                }
                else
                {
                    Version = $"v.{Parameters.CommitHash}-Local";
                }
            }
            else
            {
                Information("在 GitHub Actions 中运行");

                Assert.True(Parameters.GhTag is not null, "在 GitHub Actions 中运行，但是不存在 Tag");

                GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(nameof(NukeBuild)))
                {
                    Credentials = new Credentials(Parameters.GitHubPersonalAccessToken)
                };

                LatestTag = Parameters.IsPreRelease ? GetLatestTag() : GetLatestReleaseTag();
                
                Version = Parameters.GhTag;
            }
        });

    Target SetVersion => _ => _
        .Executes(() =>
        {
            Information($"当前版本号设置为：{Version}");
        });
}
