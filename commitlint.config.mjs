export default {
  extends: ["@commitlint/config-conventional"],
  rules: {
    "subject-case": [0],
    "header-max-length": [2, "always", 200],
    "type-enum": [
      2,
      "always",
      [
        "feat",
        "fix",
        "docs",
        "style",
        "refactor",
        "perf",
        "test",
        "chore",
        "ci",
        "build",
        "revert",
      ],
    ],
  },
  ignores: [
    (message) => /^Merge pull request/.test(message),
    (message) => message.includes("squash"),
  ],
};
