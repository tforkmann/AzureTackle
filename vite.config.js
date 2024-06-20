import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

/** @type {import('vite').UserConfig} */

export default defineConfig(({ command, mode, ssrBuild }) => {
    var isDev = command === 'serve'
    return {
            plugins: [react()],
            base: isDev ? undefined : '/AzureTackle/',
            root: "./src/docs",
            server: {
            port: 8080,
            proxy: {
            '/api': 'http://localhost:5000',
        }
        },
            build: {
            outDir:"../../publish/docs"
        },
        define: {
            "process.env": process.env,
            // // By default, Vite doesn't include shims for NodeJS/
            // // necessary for segment analytics lib to work
            "global": {},
        },

    }
})
