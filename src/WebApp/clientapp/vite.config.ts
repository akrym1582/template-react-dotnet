import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { promises as fs } from 'fs'
import path from 'path'
import { promisify } from 'util'
import { fileURLToPath } from 'url'
import { brotliCompress, constants, gzip } from 'zlib'
import type { Plugin } from 'vite'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const gzipAsync = promisify(gzip)
const brotliCompressAsync = promisify(brotliCompress)

function precompressAssets(): Plugin {
  return {
    name: 'precompress-assets',
    async closeBundle() {
      const distPath = path.resolve(__dirname, 'dist')
      const files = await collectFiles(distPath)

      await Promise.all(
        files.map(async (filePath) => {
          const source = await fs.readFile(filePath)
          const [gzipped, brotlied] = await Promise.all([
            gzipAsync(source, { level: constants.Z_BEST_COMPRESSION }),
            brotliCompressAsync(source, {
              params: {
                [constants.BROTLI_PARAM_QUALITY]: constants.BROTLI_MAX_QUALITY,
              },
            }),
          ])

          await Promise.all([
            fs.writeFile(`${filePath}.gz`, gzipped),
            fs.writeFile(`${filePath}.br`, brotlied),
          ])
        }),
      )
    },
  }
}

async function collectFiles(directoryPath: string): Promise<string[]> {
  const entries = await fs.readdir(directoryPath, { withFileTypes: true })
  const nestedFiles = await Promise.all(
    entries.map(async (entry) => {
      const entryPath = path.join(directoryPath, entry.name)
      if (entry.isDirectory()) {
        return collectFiles(entryPath)
      }

      if (entry.isFile() && !entry.name.endsWith('.gz') && !entry.name.endsWith('.br')) {
        return [entryPath]
      }

      return []
    }),
  )

  return nestedFiles.flat()
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss(), precompressAssets()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test-setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
    },
  },
})
