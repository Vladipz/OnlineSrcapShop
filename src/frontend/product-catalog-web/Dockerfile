FROM node:24.15.0-bookworm-slim AS build
WORKDIR /source
COPY src/frontend/product-catalog-web/package.json src/frontend/product-catalog-web/package-lock.json ./
RUN npm ci
COPY src/frontend/product-catalog-web/ ./
RUN npm run build

FROM nginx:1.30.5-alpine AS runtime
COPY deployment/nginx/nginx.conf /etc/nginx/nginx.conf
COPY deployment/nginx/http.conf /etc/nginx/conf.d/default.conf
COPY deployment/nginx/app-locations.conf /etc/nginx/app-locations.conf
COPY --from=build /source/dist/product-catalog-web/browser /usr/share/nginx/html
USER nginx
EXPOSE 8080
ENTRYPOINT ["nginx", "-g", "daemon off;"]
