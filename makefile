dc = docker compose

options-demo = -f ./docker/compose-demo.yml
options-api-test = -f ./docker/compose-api-test.yml
options-selenium-test = -f ./docker/compose-selenium-test.yml

demo:
	$(dc) $(options-demo) build api-build webapp-build
	$(dc) $(options-demo) restart
	$(dc) $(options-demo) up -d

demo-stop:
	$(dc) $(options-demo) stop

demo-down:
	$(dc) $(options-demo) down

api-test:
	$(dc) $(options-api-test) build testing-api-build testing-api-test
	$(dc) $(options-api-test) up -d
	$(dc) $(options-api-test) logs -f testing-api-test
	$(dc) $(options-api-test) down

web-test:
	$(dc) $(options-selenium-test) build webapp-build-selenium selenium-tests
	$(dc) $(options-selenium-test) up -d
	$(dc) $(options-selenium-test) logs -f selenium-tests
	$(dc) $(options-selenium-test) down

tests: api-test web-test
