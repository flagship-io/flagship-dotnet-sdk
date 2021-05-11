var app = new Vue({
  el: "#app",
  data: {
    envId: "",
    apiKey: "",
    timeout: 2000,
    pollingInterval: 60000,
    bucketing: true,
    visitorId: "test-visitor",
    context: "{\n}",
    envOk: false,
    envError: null,
    visitorOk: false,
    visitorError: null,
    eventOk: false,
    eventError: null,
    data: null,
    hit: { t: "EVENT" },
    hitTypes: ["EVENT", "TRANSACTION", "ITEM", "PAGEVIEW", "SCREENVIEW"],
    flag: { name: "", type: "bool", defaultValue: "", activate: true },
    flagOk: false,
    flagInfo: { name: "" },
    flagInfoOk: false,
  },
  methods: {
    getEnv() {
      this.$http.get("/env").then((response) => {
        // get body data
        this.currentEnv = response.body;
        this.bucketing = response.body.bucketing;
        this.envId = response.body.environment_id;
        this.apiKey = response.body.api_key;
        this.timeout = response.body.timeout;
        this.pollingInterval = response.body.pollingInterval;
      });
    },
    setEnv() {
      this.envOk = false;
      this.envError = null;
      this.$http
        .put("/env", {
          environment_id: this.envId,
          api_key: this.apiKey,
          bucketing: this.bucketing,
          timeout: this.timeout || 0,
          polling_interval: this.pollingInterval || 0,
        })
        .then(
          (response) => {
            this.envOk = true;
          },
          (response) => {
            this.envOk = false;
            this.envError = response.body;
            this.envError.error = Object.keys(response.body.errors)
              .map((k) => response.body.errors[k])
              .join(" ");
          }
        );
    },
    setVisitor() {
      this.visitorOk = false;
      this.visitorError = null;
      this.data = null;

      this.$http
        .put("/visitor", {
          visitor_id: this.visitorId,
          context: this.context ? JSON.parse(this.context) : null,
        })
        .then(
          (response) => {
            // get body data
            this.data = response.body;
            this.visitorOk = true;
          },
          (response) => {
            this.visitorOk = false;
            this.visitorError = response.body;
            if (response.body.errors) {
              this.visitorError.error = Object.keys(response.body.errors)
                .map((k) => response.body.errors[k])
                .join(" ");
            }
          }
        );
    },
    changeType(e) {
      this.hit = {
        t: this.hit.t,
      };
    },
    sendHit() {
      this.eventOk = false;
      this.eventError = null;

      this.$http.post("/hit", this.hit).then(
        () => {
          this.eventOk = true;
        },
        (response) => {
          this.eventOk = false;
          this.eventError = response.body;
        }
      );
    },
    getFlag() {
      this.flagOk = false;

      const { name, type, activate, defaultValue } = this.flag;

      if (!name || !type) {
        this.flagOk = { err: "Missing flag name or type" };
        return;
      }

      this.$http
        .get(
          `/flag/${name}?type=${type}&activate=${activate}&defaultValue=${defaultValue}`
        )
        .then(
          (response) => {
            this.flagOk = response.body;
          },
          (response) => {
            this.flagOk = response.body;
          }
        );
    },
    getFlagInfo() {
      this.flagInfoOk = false;

      const { name } = this.flagInfo;

      if (!name) {
        this.flagInfoOk = { err: "Missing flag name or type" };
        return;
      }

      this.$http.get(`/flag/${name}/info`).then(
        (response) => {
          console.log(response.body.value);
          this.flagInfoOk = response.body.value;
        },
        (response) => {
          console.log("youpi");
          this.flagInfoOk = response.body;
        }
      );
    },
  },
  mounted() {
    this.getEnv();
  },
});
