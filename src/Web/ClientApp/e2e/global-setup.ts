import { spawn, type ChildProcessByStdio } from 'node:child_process';
import type { Readable, Writable } from 'node:stream';

type AspireHostProcess = ChildProcessByStdio<Writable, Readable, null>;
import { createInterface } from 'node:readline';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../../../', import.meta.url));
const startupTimeoutMs = 300_000;

export default async function globalSetup() {
  if (process.env.E2E_BASE_URL) {
    return;
  }

  const aspireHost = spawn(
    'dotnet',
    ['run', '--project', 'tests/AcceptanceTestHost', '--no-launch-profile'],
    { cwd: repoRoot, stdio: ['pipe', 'pipe', 'inherit'], shell: process.platform === 'win32' }
  );

  process.env.E2E_BASE_URL = await waitForBaseUrl(aspireHost);

  return async () => {
    // Closing stdin is the host's shutdown signal.
    aspireHost.stdin.end();

    await new Promise<void>((resolve) => {
      const forceKill = setTimeout(() => aspireHost.kill('SIGKILL'), 30_000);
      aspireHost.once('exit', () => {
        clearTimeout(forceKill);
        resolve();
      });
    });
  };
}

function waitForBaseUrl(host: AspireHostProcess): Promise<string> {
  return new Promise((resolve, reject) => {
    const onExit = (code: number | null) =>
      fail(new Error(`Aspire host exited with code ${code} before reporting a frontend URL`));

    const timeout = setTimeout(
      () => fail(new Error(`Aspire host did not report a frontend URL within ${startupTimeoutMs}ms`)),
      startupTimeoutMs
    );

    const cleanup = () => {
      clearTimeout(timeout);
      host.off('exit', onExit);
    };

    const fail = (error: Error) => {
      cleanup();
      reject(error);
    };

    host.once('exit', onExit);

    createInterface({ input: host.stdout }).on('line', (line) => {
      console.log(`[aspire] ${line}`);
      const match = /^E2E_BASE_URL=(.+)$/.exec(line);
      if (match) {
        cleanup();
        resolve(match[1].trim());
      }
    });
  });
}
