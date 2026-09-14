# SQLite instead of PostgreSQL

The old app ran on PostgreSQL. For a single-household, internally-hosted app, that's more operational weight than the workload needs, so we're switching to SQLite — also `Ardalis.CleanArchitecture.Template`'s own default provider. .NET Aspire orchestration is retained regardless, since it's an independent toggle on the template and still useful for composing the API and frontend together.
