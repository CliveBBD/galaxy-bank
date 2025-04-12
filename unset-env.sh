#!/bin/bash

# Loop over keys in .env and unset them
while IFS='=' read -r key _; do
  # Skip empty lines and comments
  [[ "$key" =~ ^#.*$ || -z "$key" ]] && continue
  unset "$key"
done < .env
