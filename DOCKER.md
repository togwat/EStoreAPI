# Compose
Development:
`docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build`

Production:
`docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d`