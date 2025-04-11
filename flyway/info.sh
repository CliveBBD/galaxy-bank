set -a  # Automatically export all sourced variables
source ../.env  # Load default environment variables
source ../.env.local  # Override with local environment variables
set +a  # Stop auto-exporting variables
flyway -url="jdbc:postgresql://$POSTGRES_HOST:$POSTGRES_PORT/$POSTGRES_DB" \
       -user="$POSTGRES_USER" \
       -password="$POSTGRES_PASSWORD" \
       -schemas="$POSTGRES_SCHEMA" \
       -configFiles=conf/flyway.conf \
       -X \
       info

