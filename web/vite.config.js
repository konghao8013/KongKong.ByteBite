import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';
export default defineConfig({
    plugins: [vue()],
    resolve: {
        alias: {
            '@': resolve(__dirname, 'src')
        }
    },
    server: {
        port: 3000,
        proxy: {
            '/api': {
                target: 'http://localhost:5044',
                changeOrigin: true
            },
            '/hubs': {
                target: 'http://localhost:5044',
                changeOrigin: true,
                ws: true
            },
            '/uploads': {
                target: 'http://localhost:5044',
                changeOrigin: true
            }
        }
    },
    css: {
        preprocessorOptions: {
            scss: {
                additionalData: `@use "@/styles/variables" as *;\n`,
                api: 'modern-compiler'
            }
        }
    }
});
