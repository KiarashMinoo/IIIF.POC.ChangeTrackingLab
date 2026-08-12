# wwwroot

Static assets served directly by `app.UseStaticFiles()`. One stylesheet, referenced by `Pages/Index.cshtml` with `asp-append-version="true"`.

## Files

| File | Types |
| --- | --- |
| [`css/site.css`](../../wwwroot/css/site.css) | — |

## Overview

`site.css` styles the single demo page: the dark `.hero` header, the six-column `.status-grid` of metrics, `.card` sections for the button rows and tables, `.json-cell`/`.kind` for the ChangeSet and event-log tables, the `.two-column` full/changed-only Manifest panes, and two `@media` breakpoints (`1150px`, `700px`) that collapse the grid and columns for narrower viewports.

Class names map directly to the markup in [`Pages/Index.cshtml`](../Pages/README.md#indexcshtml) — there's no build step or preprocessor between the two.

## Usage recipe

Adding a new visual element to the page means adding its class to `site.css` directly; there's no CSS bundler or SCSS pipeline in this project.
