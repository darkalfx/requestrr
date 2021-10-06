import React from "react";
import Loader from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { testSonarrSettings } from "../../../store/actions/SonarrClientActions"
import { setSonarrConnectionSettings } from "../../../store/actions/SonarrClientActions"
import ValidatedTextbox from "../../Inputs/ValidatedTextbox"
import Textbox from "../../Inputs/Textbox"
import Dropdown from "../../Inputs/Dropdown"
import SonarrCategoryList from "./SonarrCategoryList"

import {
  FormGroup,
  Input,
  Row,
  Col
} from "reactstrap";

class Sonarr extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isTestingSettings: false,
      testSettingsRequested: false,
      testSettingsSuccess: false,
      testSettingsError: "",
      hostname: "",
      isHostnameValid: false,
      port: "7878",
      isPortValid: false,
      apiKey: "",
      isApiKeyValid: false,
      useSSL: false,
      apiVersion: "",
      baseUrl: "",
      searchNewRequests: true,
      monitorNewRequests: true,
    };

    this.onTestSettings = this.onTestSettings.bind(this);
    this.onUseSSLChanged = this.onUseSSLChanged.bind(this);
    this.onValueChange = this.onValueChange.bind(this);
    this.onValidate = this.onValidate.bind(this);
    this.updateStateFromProps = this.updateStateFromProps.bind(this);
    this.validateNonEmptyString = this.validateNonEmptyString.bind(this);
    this.validatePort = this.validatePort.bind(this);
    this.validateCategoryName = this.validateCategoryName.bind(this);
  }

  componentDidMount() {
    this.updateStateFromProps(this.props);
  }

  componentDidUpdate(prevProps) {
    var previousNames = prevProps.settings.categories.map(x => x.name);
    var currentNames = this.props.settings.categories.map(x => x.name);

    if (!(prevProps.settings.profiles.length == this.props.settings.profiles.length && prevProps.settings.profiles.reduce((a, b, i) => a && this.props.settings.profiles[i], true))
      || !(prevProps.settings.paths.length == this.props.settings.paths.length && prevProps.settings.paths.reduce((a, b, i) => a && this.props.settings.paths[i], true))
      || !(prevProps.settings.languages.length == this.props.settings.languages.length && prevProps.settings.languages.reduce((a, b, i) => a && this.props.settings.languages[i], true))
      || !(previousNames.length == currentNames.length && currentNames.every((value, index) => previousNames[index] == value))) {
      this.onValueChange();
    }
  }

  updateStateFromProps = props => {
    this.setState({
      isTestingSettings: false,
      TestingSettings: false,
      testSettingsRequested: false,
      testSettingsSuccess: false,
      testSettingsError: "",
      hostname: props.settings.hostname,
      isHostnameValid: false,
      port: props.settings.port,
      isPortValid: false,
      apiKey: props.settings.apiKey,
      isApiKeyValid: false,
      useSSL: props.settings.useSSL,
      apiVersion: props.settings.version,
      baseUrl: props.settings.baseUrl,
      searchNewRequests: props.settings.searchNewRequests,
      monitorNewRequests: props.settings.monitorNewRequests,
    }, this.onValueChange);
  }

  onUseSSLChanged = event => {
    this.setState({
      useSSL: !this.state.useSSL
    }, this.onValueChange);
  }

  validateNonEmptyString = value => {
    return /\S/.test(value);
  }

  validatePort = value => {
    return /^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$/.test(value);
  }

  onTestSettings = e => {
    e.preventDefault();

    if (!this.state.isTestingSettings
      && this.state.isHostnameValid
      && this.state.isPortValid
      && this.state.isApiKeyValid) {
      this.setState({ isTestingSettings: true });

      this.props.testSettings({
        hostname: this.state.hostname,
        baseUrl: this.state.baseUrl,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            this.setState({
              testSettingsRequested: true,
              testSettingsError: "",
              testSettingsSuccess: true
            });
          }
          else {
            var error = "An unknown error occurred while testing the settings";

            if (typeof (data.error) === "string")
              error = data.error;

            this.setState({
              testSettingsRequested: true,
              testSettingsError: error,
              testSettingsSuccess: false
            });
          }

          this.setState({ isTestingSettings: false });
        });
    }
  }

  onValueChange() {
    this.props.setConnectionSettings({
      hostname: this.state.hostname,
      baseUrl: this.state.baseUrl,
      port: this.state.port,
      apiKey: this.state.apiKey,
      useSSL: this.state.useSSL,
      version: this.state.apiVersion,
    });

    this.props.onChange({
      hostname: this.state.hostname,
      baseUrl: this.state.baseUrl,
      port: this.state.port,
      apiKey: this.state.apiKey,
      useSSL: this.state.useSSL,
      version: this.state.apiVersion,
      searchNewRequests: this.state.searchNewRequests,
      monitorNewRequests: this.state.monitorNewRequests,
    });

    this.onValidate();
  }

  onValidate() {
    var isLanguageValid = this.state.apiVersion !== "2" ? this.props.settings.areLanguagesValid : true;

    this.props.onValidate(this.state.isApiKeyValid
      && this.state.isHostnameValid
      && this.state.isPortValid
      && this.props.settings.areProfilesValid
      && this.props.settings.arePathsValid
      && this.props.settings.categories.every(x => this.validateCategoryName(x.name))
      && isLanguageValid);
  }

  validateCategoryName(value) {
    if (!/\S/.test(value)) {
      return false;
    }
    else if (/^[\w-]{1,32}$/.test(value)) {
      var names = this.props.settings.categories.map(x => x.name);

      if (new Set(names).size !== names.length) {
        return false;
      }
    }
    else {
      return false;
    }
    return true;
  }

  render() {
    return (
      <>
        <div>
          <h6 className="heading-small text-muted mb-4">
            Sonarr Connection Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <Dropdown
                name="API"
                value={this.state.apiVersion}
                items={[{ name: "Version 2", value: "2" }, { name: "Version 3", value: "3" }]}
                onChange={newApiVersion => this.setState({ apiVersion: newApiVersion }, this.onValueChange)} />
            </Col>
            <Col lg="6">
              <ValidatedTextbox
                name="API Key"
                placeholder="Enter api key"
                alertClassName="mt-3 mb-0"
                errorMessage="api key is required."
                isSubmitted={this.props.isSubmitted}
                value={this.state.apiKey}
                validation={this.validateNonEmptyString}
                onChange={newApiKey => this.setState({ apiKey: newApiKey }, this.onValueChange)}
                onValidate={isValid => this.setState({ isApiKeyValid: isValid }, this.onValidate)} />
            </Col>
          </Row>
          <Row>
            <Col lg="6">
              <ValidatedTextbox
                name="Host or IP"
                placeholder="Enter host or ip"
                alertClassName="mt-3 mb-0"
                errorMessage="Hostname is required."
                isSubmitted={this.props.isSubmitted}
                value={this.state.hostname}
                validation={this.validateNonEmptyString}
                onChange={newHostname => this.setState({ hostname: newHostname }, this.onValueChange)}
                onValidate={isValid => this.setState({ isHostnameValid: isValid }, this.onValidate)} />
            </Col>
            <Col lg="6">
              <ValidatedTextbox
                name="Port"
                placeholder="Enter port"
                alertClassName="mt-3 mb-0"
                errorMessage="Please enter a valid port."
                isSubmitted={this.props.isSubmitted}
                value={this.state.port}
                validation={this.validatePort}
                onChange={newPort => this.setState({ port: newPort }, this.onValueChange)}
                onValidate={isValid => this.setState({ isPortValid: isValid }, this.onValidate)} />
            </Col>
          </Row>
          <Row>
            <Col lg="6">
              <Textbox
                name="Base Url"
                placeholder="Enter base url configured in Sonarr, leave empty if none configured."
                value={this.state.baseUrl}
                onChange={newBaseUrl => this.setState({ baseUrl: newBaseUrl }, this.onValueChange)} />
            </Col>
            <Col lg="6">
            </Col>
          </Row>
          <Row>
            <Col lg="6">
              <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                <Input
                  className="custom-control-input"
                  id="useSSL"
                  type="checkbox"
                  onChange={this.onUseSSLChanged}
                  checked={this.state.useSSL}
                />
                <label
                  className="custom-control-label"
                  htmlFor="useSSL">
                  <span className="text-muted">Use SSL</span>
                </label>
              </FormGroup>
            </Col>
            <Col lg="6"></Col>
          </Row>
          <Row>
            <Col>
              <FormGroup className="mt-4">
                {
                  this.state.testSettingsRequested && !this.state.isTestingSettings ?
                    !this.state.testSettingsSuccess ? (
                      <Alert className="text-center" color="danger">
                        <strong>{this.state.testSettingsError}</strong>
                      </Alert>)
                      : <Alert className="text-center" color="success">
                        <strong>The specified settings are valid.</strong>
                      </Alert>
                    : null
                }
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup className="text-right">
                <button onClick={this.onTestSettings} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} className="btn btn-icon btn-3 btn-default" type="button">
                  <span className="btn-inner--icon">
                    {
                      this.state.isTestingSettings ? (
                        <Loader
                          className="loader"
                          type="Oval"
                          color="#11cdef"
                          height={19}
                          width={19}
                        />)
                        : (<i className="fas fa-cogs"></i>)
                    }</span>
                  <span className="btn-inner--text">Test Settings</span>
                </button>
              </FormGroup>
            </Col>
          </Row>
        </div>
        <SonarrCategoryList isSubmitted={this.props.isSubmitted} isSaving={this.props.isSaving} apiVersion={this.state.apiVersion} canConnect={this.state.isHostnameValid && this.state.isPortValid && this.state.isApiKeyValid} />
        <div>
          <h6 className="heading-small text-muted mt-4">
            Sonarr Requests Permissions Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                <Input
                  className="custom-control-input"
                  id="MonitorNewRequests"
                  type="checkbox"
                  onChange={e => { this.setState({ monitorNewRequests: !this.state.monitorNewRequests }, this.onValueChange); }}
                  checked={this.state.monitorNewRequests}
                />
                <label
                  className="custom-control-label"
                  htmlFor="MonitorNewRequests">
                  <span className="text-muted">Automatically monitor newly added tv shows</span>
                </label>
              </FormGroup>
              <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                <Input
                  className="custom-control-input"
                  id="SearchNewRequests"
                  type="checkbox"
                  onChange={e => { this.setState({ searchNewRequests: !this.state.searchNewRequests }, this.onValueChange); }}
                  checked={this.state.searchNewRequests}
                />
                <label
                  className="custom-control-label"
                  htmlFor="SearchNewRequests">
                  <span className="text-muted">Automatically search for episodes when a request is made</span>
                </label>
              </FormGroup>
            </Col>
          </Row>
        </div>
      </>
    );
  }
}

const mapPropsToState = state => {
  return {
    settings: state.tvShows.sonarr
  }
};

const mapPropsToAction = {
  testSettings: testSonarrSettings,
  setConnectionSettings: setSonarrConnectionSettings,
};

export default connect(mapPropsToState, mapPropsToAction)(Sonarr);