# LiftAndShift — ClientApp

Nuxt 4 frontend for the LiftAndShift API. Normally launched for you via `LiftAndShift.AspireHost`
(`dotnet run --project src/LiftAndShift.AspireHost`), not run standalone.

## Standalone dev

```bash
npm install
npm run dev   # http://localhost:3000
```

The API must also be running (`dotnet run --project src/LiftAndShift.Web`) — its plain-HTTP dev
endpoint (`http://localhost:57680` by default, see `Properties/launchSettings.json`) is what
`nuxt.config.ts`'s `runtimeConfig.public.apiBase` points at when not overridden by
`NUXT_PUBLIC_API_BASE` (which Aspire injects automatically).

## Stack

- Nuxt 4 (Vue 3, SSR)
- Tailwind CSS v4 (`@tailwindcss/vite`)
- Pinia
- Design tokens in `app/assets/css/main.css` — see `CONTEXT.md` > Branding for the source palette
