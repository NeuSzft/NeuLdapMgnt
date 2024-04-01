dc = docker compose

demo-options = -f compose-demo.yml
api-test-options = -f compose-demo.yml

demo:
	$(dc) $(demo-options) build api-build webapp-build
	$(dc) $(demo-options) restart
	$(dc) $(demo-options) up -d

demo-stop:
	$(dc) $(demo-options) stop

demo-down:
	$(dc) $(demo-options) down
