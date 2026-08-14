# TravelCore.Configuration

Narrow helpers for **explicit** Options registration.

- Own configuration with the capability that consumes it
- Pass the section name explicitly — no reflection/section-name magic
- Use `ValidateDataAnnotations` + `ValidateOnStart` when a section is mandatory for that capability

Do not create a giant `TravelCoreOptions` / `AllSettings` container here.
