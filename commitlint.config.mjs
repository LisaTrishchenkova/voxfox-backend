export default {
  extends: ["@commitlint/config-conventional"],
  ignores: [
    (message) => /^Merge/.test(message),
    (message) => /\(#\d+\)$/.test(message),
  ],
};
