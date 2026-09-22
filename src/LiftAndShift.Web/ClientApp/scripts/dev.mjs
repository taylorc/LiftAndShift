// Runs `nuxt dev` with NODE_EXTRA_CA_CERTS pointed at the ASP.NET Core HTTPS dev cert, so Node's
// SSR fetch trusts it (Node doesn't consult the OS/`dotnet dev-certs --trust` store on its own).
// Exports the cert on first run (or whenever it's missing) via `dotnet dev-certs https`.
import { spawnSync } from 'node:child_process'
import { existsSync, mkdirSync } from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const clientAppDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)))
const certPath = path.join(clientAppDir, '.certs', 'aspnetcore-dev-cert.pem')

if (!existsSync(certPath)) {
  mkdirSync(path.dirname(certPath), { recursive: true })
  console.log('Exporting the ASP.NET Core HTTPS dev cert for Node to trust...')

  const exportResult = spawnSync(
    'dotnet',
    ['dev-certs', 'https', '--export-path', certPath, '--format', 'Pem', '--no-password'],
    { stdio: 'inherit', shell: true }
  )

  if (exportResult.status !== 0) {
    console.error(
      'Could not export the dev cert. Run `dotnet dev-certs https --trust` once, then retry `npm run dev`.'
    )
    process.exit(exportResult.status ?? 1)
  }
}

const devResult = spawnSync('npx', ['nuxt', 'dev'], {
  stdio: 'inherit',
  shell: true,
  env: { ...process.env, NODE_EXTRA_CA_CERTS: certPath }
})

process.exit(devResult.status ?? 0)
