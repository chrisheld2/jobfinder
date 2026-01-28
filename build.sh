#!/bin/bash

set -e

echo "Building Job Finder..."

# Build Frontend
echo "📦 Building Vue 3 frontend..."
cd JobFinderUI
npm install
npm run build
cd ..

echo "✅ Frontend build complete"
echo ""

# Clean backend wwwroot (optional)
# rm -rf JobFinderApi/wwwroot/*

echo "✅ Build complete! The frontend files are in JobFinderApi/wwwroot"
echo ""
echo "To run the application:"
echo "  cd JobFinderApi"
echo "  dotnet run"
