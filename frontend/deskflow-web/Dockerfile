FROM node:24-alpine AS build
WORKDIR /app
COPY frontend/deskflow-web/package*.json ./
RUN npm install
COPY frontend/deskflow-web/ .
ARG VITE_API_URL=http://localhost:8080
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginx:1.29-alpine
COPY frontend/deskflow-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
