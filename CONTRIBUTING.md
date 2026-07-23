# Contributing to ZipAll

ZipAll is a small, single-maintainer Windows utility, but issues and pull
requests are welcome.

## Ground rules

- **C# (.NET 8) only**, WinForms for the UI. No WPF, no Avalonia, no
  third-party UI/component libraries.
- **English only** in source, comments-equivalent naming, commit messages,
  and issue/PR text. Variable, method, and class names must be in English.
- **No comments in source code.** Keep code self-explanatory through naming
  and small, focused methods instead.
- **White theme only** — don't add dark mode or alternate themes.
- **Windows-only.** Don't add cross-platform abstractions or conditional
  compilation for other OSes.
- **Archive format via the BCL only** (`System.IO.Compression`). Don't
  re-implement DEFLATE/CRC32/Zip64 or take a dependency on a third-party
  archive library.
- **Self-contained deployment.** Any new dependency must be embeddable in the
  self-contained, single-file publish — no dependency that requires a
  separate install on the target machine.

## Before you open a PR

1. Check [`ROADMAP.md`](ROADMAP.md) (kept locally, not published to GitHub)
   or open an issue first if you're proposing a new feature, so it can be
   discussed before you invest time in it.
2. Keep changes focused — one feature or fix per PR.
3. Update [`CHANGELOG.md`](CHANGELOG.md) (Keep a Changelog format) under
   `[Unreleased]` with a short, user-facing description of what changed.
4. Update [`README.md`](README.md) if you added or changed user-facing
   behavior.
5. Build and run the existing manual test harnesses under `tests/`
   (`ManualHarness`, `ExclusionHarness`) to confirm nothing regressed, in
   addition to any new tests your change needs.

## Reporting bugs

Please include:
- Windows version and architecture (x64/ARM64).
- ZipAll version (About tab, or `ZipAll --version` is not currently
  supported — check the About tab).
- Steps to reproduce, and whether the source folder has anything unusual
  (very long paths, locked files, huge file counts, etc.).

## Contact

Patrick JAILLET — contact.shaderstudio@gmail.com
