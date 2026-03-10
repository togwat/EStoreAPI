# EStoreAPI.client/Dockerfile

# build
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
# clean install
RUN npm ci
COPY . .
RUN npm run build

# runtime
FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
# port
EXPOSE 80