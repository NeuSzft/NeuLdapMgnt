dc = docker compose

options-prod = -f ./docker/compose-prod.yml --env-file .env
options-demo = -f ./docker/compose-demo.yml
options-api-tests = -f ./docker/compose-api-tests.yml
options-selenium-tests = -f ./docker/compose-selenium-tests.yml
options-model-tests = -f ./docker/compose-model-tests.yml

start: env
	$(dc) $(options-prod) build prod-api-build prod-webapp-build
	$(dc) $(options-prod) restart
	$(dc) $(options-prod) up -d

stop:
	$(dc) $(options-prod) stop

down:
	$(dc) $(options-prod) down

env:
ifeq ($(wildcard ./.env),)
	$(file >  .env,# Environment variables used for deployment)
	$(file >> .env,)
	$(file >> .env,LDAP_ORGANIZATION = Neu)
	$(file >> .env,LDAP_DOMAIN = neu.local)
	$(file >> .env,LDAP_ADMIN_PASSWORD = ldappass)
	$(file >> .env,)
	$(file >> .env,POSTGRES_USER_PASSWORD = postgres)
	$(file >> .env,)
	$(file >> .env,DEFAULT_ADMIN_USERNAME = admin)
	$(file >> .env,DEFAULT_ADMIN_PASSWORD = adminpass)
	$(file >> .env,)
	$(file >> .env,LOG_TO_DB = true)
	$(file >> .env,)
	$(file >> .env,PORT = 80)
	@echo Created .env file
else
	@echo The .env file already exists
endif

demo:
	$(dc) $(options-demo) build demo-api-build demo-webapp-build
	$(dc) $(options-demo) restart
	$(dc) $(options-demo) up -d

demo-stop:
	$(dc) $(options-demo) stop

demo-down:
	$(dc) $(options-demo) down

model-tests:
	$(dc) $(options-model-tests) build model-tests
	$(dc) $(options-model-tests) up -d
	$(dc) $(options-model-tests) logs -f model-tests
	$(dc) $(options-model-tests) down

api-tests:
	$(dc) $(options-api-tests) build api-test-api-build api-test-tests
	$(dc) $(options-api-tests) up -d
	$(dc) $(options-api-tests) logs -f api-test-tests
	$(dc) $(options-api-tests) down

web-tests:
	$(dc) $(options-selenium-tests) build selenium-webapp-build selenium-tests selenium-api-build selenium-api
	$(dc) $(options-selenium-tests) up -d
	$(dc) $(options-selenium-tests) logs -f selenium-tests
	$(dc) $(options-selenium-tests) down -v

tests: | model-tests api-tests web-tests
