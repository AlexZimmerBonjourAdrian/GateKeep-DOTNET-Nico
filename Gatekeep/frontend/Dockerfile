# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Accept build argument for API URL (CRÍTICO: debe estar disponible en build time)
# Valor por defecto: https://api.zimmzimmgames.com
ARG NEXT_PUBLIC_API_URL=https://api.zimmzimmgames.com
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

# Copy package files
COPY package*.json ./

# Copy scripts for postinstall
COPY scripts ./scripts

# Install dependencies
RUN npm ci

# Copy source code and configuration
COPY . .

# Log the API URL being used in the build
RUN echo "Building with NEXT_PUBLIC_API_URL: $NEXT_PUBLIC_API_URL"

# Build the application
RUN npm run build

# Production stage
FROM node:18-alpine AS production

WORKDIR /app

# Copy package files
COPY package*.json ./

# Copy scripts for postinstall
COPY scripts ./scripts

# Install dependencies (including next which is needed for next start)
RUN npm ci --production

# Copy built application and necessary files from build stage
# IMPORTANTE: next.config.js tiene distDir: 'out', pero next start busca '.next' por defecto
# Copiamos 'out' a '.next' y ajustamos el next.config.js para usar '.next'
COPY --from=build /app/out ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/package.json ./package.json
COPY --from=build /app/next.config.js ./next.config.js

# Modificar next.config.js para cambiar distDir: 'out' a distDir: '.next'
# Usamos sed que es más confiable en Alpine Linux
RUN sed -i "s/distDir: 'out'/distDir: '.next'/g" ./next.config.js || \
    sed -i "s/distDir: \"out\"/distDir: \".next\"/g" ./next.config.js

# Cache directory is optional, skip if it doesn't exist
RUN mkdir -p ./node_modules/.cache || true

# Expose port
EXPOSE 3000

# Set environment variables
ENV NODE_ENV=production
ENV PORT=3000

# Start the application
CMD ["npm", "start"]

