# Use the official Node.js image with Alpine for a smaller footprint
FROM node:18-alpine

# Set the working directory inside the container
WORKDIR /app

# Install Expo CLI globally
RUN npm install -g @expo/cli
RUN npm install @expo/ngrok@latest
# Copy package.json and package-lock.json (or yarn.lock) first for better caching
COPY package*.json ./

# Install dependencies
RUN npm install


# Copy the rest of the application code into the container
COPY . .

# Expose the ports that Expo uses
EXPOSE 19000  
EXPOSE 19001  
EXPOSE 19002  

# Start the Expo project
CMD ["npx", "expo", "start", "--tunnel"]
