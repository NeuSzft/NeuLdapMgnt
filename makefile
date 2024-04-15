dc = docker compose

options-prod = -f ./docker/compose-prod.yml --env-file .env
options-demo = -f ./docker/compose-demo.yml
options-api-tests = -f ./docker/compose-api-test.yml
options-selenium-tests = -f ./docker/compose-selenium-test.yml

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
	$(file >> .env,DEFAULT_ADMIN_USERNAME = admin)
	$(file >> .env,DEFAULT_ADMIN_PASSWORD = adminpass)
	$(file >> .env,PORT = 80)
	$(file >> .env,API_LOGS_DIR = $(abspath $(dir .))/logs)
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

api-tests:
	$(dc) $(options-api-tests) build testing-api-build testing-api-tests
	$(dc) $(options-api-tests) up -d
	$(dc) $(options-api-tests) logs -f testing-api-tests
	$(dc) $(options-api-tests) down

web-tests:
	$(dc) $(options-selenium-tests) build webapp-build-selenium selenium-tests api-build-selenium api-selenium
	$(dc) $(options-selenium-tests) up -d
	$(dc) $(options-selenium-tests) logs -f selenium-tests
	$(dc) $(options-selenium-tests) down

tests: | api-test web-test
