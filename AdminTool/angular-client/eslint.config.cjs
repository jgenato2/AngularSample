// Flat ESLint config for Angular 21+ with angular-eslint
const angularEslint = require("@angular-eslint/eslint-plugin");
const angularTemplate = require("@angular-eslint/eslint-plugin-template");
const js = require("@eslint/js");
const tseslint = require("@typescript-eslint/eslint-plugin");
const tsparser = require("@typescript-eslint/parser");

/** @type {import('eslint').Linter.FlatConfig[]} */
module.exports = [
  js.configs.recommended,
  {
    files: ["**/*.ts"],
    languageOptions: {
      parser: tsparser,
      parserOptions: {
        project: ["./tsconfig.lint.json"],
        tsconfigRootDir: __dirname,
        sourceType: "module"
      },
      globals: {
        setTimeout: "readonly",
        clearTimeout: "readonly",
        setInterval: "readonly",
        clearInterval: "readonly",
        window: "readonly",
        document: "readonly",
        console: "readonly"
      }
    },
    plugins: {
      "@typescript-eslint": tseslint,
      "@angular-eslint": angularEslint
    },
    rules: {
      ...tseslint.configs.recommended.rules,
      ...angularEslint.configs.recommended.rules
    }
  },
  {
    files: ["**/*.spec.ts"],
    languageOptions: {
      globals: {
        describe: "readonly",
        it: "readonly",
        expect: "readonly",
        beforeEach: "readonly",
        afterEach: "readonly",
        jest: "readonly",
        jasmine: "readonly"
      }
    }
  },
  {
    files: ["**/*.html"],
    languageOptions: {
      parser: require("@angular-eslint/template-parser")
    },
    plugins: {
      "@angular-eslint/template": angularTemplate
    },
    rules: {
      ...angularTemplate.configs.recommended.rules
    }
  }
];
