# Contribution Guidelines

Before you start contributing to the project, please read the following guidelines. We follow the below guidelines to contribute to this repository.

## How To Contribute

* **DO** submit all changes via pull requests (PRs). They will be reviewed and potentially merged after a peer review from at least one maintainer.
* **DO** give PRs short but descriptive names.
* **DO** write a useful but brief description of what the PR is for.
* **DO** ensure each commit successfully builds. The entire PR must pass all checks before it will be merged.
* **DO** address PR feedback in additional commits instead of amending.
* **DO NOT** submit "work in progress" PRs. Please mark them as *Draft*. A PR should only be submitted when it is considered ready for review.
* **DO NOT** mix independent and unrelated changes in one PR.
* If there is a major upgrade or a feature addition to the project, it might be a good idea to get started with a Github issue or a Github discussion to discuss the feature or the upgrade before starting a PR on the upgrade.

## Pull Requests

We use pull requests to review and merge code into the `main` branch.
Please follow the steps below to create a pull request:

1. Fork the repository from the `main` branch ( Refer steps to [create a fork]https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo)). We use `main` branch only for sync'ing forks. All development contributions should be made to the `development` branch.
1. Create a new branch (example `development`) in your forked repository for your feature or bug fix. Detailed branching and development strategy is outline in [this guide](docs/setup-guide.md#diagram-for-forking-and-syncing).
1. Make sure the pre-commit hook is installed and working:
   1. Install pre-commit using this [link](https://pre-commit.com/#installation)
   1. Run `pre-commit run --all-files` from the root of the repository.
   1. Follow [these](https://github.com/pocc/pre-commit-hooks?tab=readme-ov-file#information-about-the-commands) instructions to install the commands
1. Run tests, linters and checks locally and make sure the pipeline is passing
1. Make sure the pipeline is passing
1. Make sure you have each PR reviewed
1. Once the PR is approved, merge it to the `main` branch, preferably using `Squash and Merge`

## Coding Style

We use [.NET source code analyzer](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8) to enforce code style.
Please make sure you have the pre-commit hook installed and working.

## VS Code

For VS Code it is recommended to install the editorconfig plugin, which will allow the code style rules defined in .editorconfig (within the root of the project repository) to be applied during development.

## Recognition for Contributions

We are happy to recognize contributors to this repository. If you have made a significant contribution, please reach out to the maintainers to be added to the list of contributors - or submit a PR to add yourself to the list.

1. John Collinson
1. Anat Balzam
1. Arpit Gaur
1. Dor Lugasi-Gal
1. Frances Tibble
1. Jack Jessel
1. Liam Moat
1. Martyna Marcinkowska
1. Nava Vaisman Levy
1. Sharon Hart
