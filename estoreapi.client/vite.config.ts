import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
    plugins: [
        react(),
        tailwindcss(),
    ],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
            'src': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '^/api': {
                target: 'https://localhost:5100',
                secure: false
            },
            '^/signin-google': {
                target: 'https://localhost:5100',
                secure: false
            },
            '^/agent': {
                target: 'http://localhost:8000',
                secure: false
            }
        },
        port: 5173,
    }
})