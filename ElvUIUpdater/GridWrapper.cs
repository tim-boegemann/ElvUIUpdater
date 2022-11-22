using LibGit2Sharp;
using System;
using System.Linq;

namespace ElvUIUpdater
{
    public class GridWrapper
    {
        public string Clone(string targetPath)
            => Repository.Clone(@"https://github.com/tukui-org/ElvUI.git", targetPath); 
        
        public void Update(string path)
        { 
            using (var repo = new Repository(path))
            {
                var trackingBranch = repo.Branches["origin/main"];
                if (trackingBranch.IsRemote)
                {
                    var branch = repo.Head;
                    repo.Branches.Update(branch, b => b.TrackedBranch = trackingBranch.CanonicalName);
                }

                PullOptions pullOptions = new PullOptions()
                {
                    MergeOptions = new MergeOptions()
                    {
                        FastForwardStrategy = FastForwardStrategy.Default
                    }
                };

                MergeResult mergeResult = Commands.Pull(
                    repo,
                    new Signature("my name", "my email", DateTimeOffset.Now),
                    pullOptions
                );
            }
        }
    }
}