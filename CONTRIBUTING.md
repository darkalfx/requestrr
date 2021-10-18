# Contributing to Requestrr

All help is welcome and greatly appreciated! If you would like to contribute to the project, the following instructions should get you started...

## Development

### Tools Required

- HTML/Typescript/Javascript editor
  - [VSCode](https://code.visualstudio.com/) is recommended. Upon opening the project, a few extensions will be automatically recommended for install.
- .NET CORE SDK 5
- NPM
- GIT

### Getting Started

1. [Fork](https://help.github.com/articles/fork-a-repo/) the repository to your own GitHub account and [clone](https://help.github.com/articles/cloning-a-repository/) it to your local device:

   ```bash
   git clone https://github.com/YOUR_USERNAME/requestrr.git
   cd requestrr/
   ```

2. Add the remote `upstream`:

   ```bash
   git remote add upstream https://github.com/darkalfx/requestrr
   ```

3. Create a new branch:

   ```bash
   git checkout -b BRANCH_NAME develop
   ```

   - It is recommended to give your branch a meaningful name, relevant to the feature or fix you are working on.
     - Good examples:
       - `docs-docker`
       - `feature-new-system`
       - `fix-title-cards`
     - Bad examples:
       - `bug`
       - `docs`
       - `feature`
       - `fix`
       - `patch`

4. Run the development environment:

   ```
    npm run install:clean
    dotnet build
   ```

   - Alternatively, you can use [Docker](https://www.docker.com/) with `docker build .`.

5. Create your patch and test your changes.

   - Be sure to follow both the [code](#contributing-code) guidelines.
   - Should you need to update your fork, you can do so by rebasing from `upstream`:
     ```bash
     git fetch upstream
     git rebase upstream/develop
     git push origin BRANCH_NAME -f
     ```

### Contributing Code

- If you are taking on an existing bug or feature ticket, please comment on the [issue](https://github.com/darkalfx/requestrr/issues) to avoid multiple people working on the same thing.
- All commits **must** follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
  - It is okay to squash your pull request down into a single commit that fits this standard.
  - Pull requests with commits not following this standard will **not** be merged.
- Please make meaningful commits, or squash them.
- Always rebase your commit to the latest `master` branch.
- It is your responsibility to keep your branch up-to-date. Your work will **not** be merged unless it is rebased off the latest `master` branch.
- You can create a "draft" pull request early to get feedback on your work.
- Format your code so that it matches the rest of the files.
- If you have questions or need help, you can reach out via [Discord server](https://discord.com/invite/ATCM64M).


## Attribution

This contribution guide was inspired by the [Overseerr](https://github.com/sct/overseerr) contribution guides.
