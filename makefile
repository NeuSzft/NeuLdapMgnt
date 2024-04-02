dc = docker compose

options-demo = -f compose-demo.yml
options-api-test = -f compose-api-test.yml

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
	@echo Nothing to do.

tests: api-test web-test
