import React from "react";
import { Oval } from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { testOmbiSettings } from "../../store/actions/MovieClientsActions"
import ValidatedTextbox from "../Inputs/ValidatedTextbox"
import Textbox from "../Inputs/Textbox"
import Dropdown from "../Inputs/Dropdown"

import {
  FormGroup,
  Input,
  Row,
  Col
} from "reactstrap";

class Ombi extends React.Component {
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
      apiUsername: "",
      useSSL: "",
      apiVersion: "",
      baseUrl: "",
    };

    this.onTestSettings = this.onTestSettings.bind(this);
    this.onUseSSLChanged = this.onUseSSLChanged.bind(this);
    this.onValueChange = this.onValueChange.bind(this);
    this.onValidate = this.onValidate.bind(this);
    this.updateStateFromProps = this.updateStateFromProps.bind(this);
    this.validateNonEmptyString = this.validateNonEmptyString.bind(this);
    this.validatePort = this.validatePort.bind(this);
  }

  componentDidMount() {
    this.updateStateFromProps(this.props);
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
      apiUsername: props.settings.apiUsername,
      baseUrl: props.settings.baseUrl,
      useSSL: props.settings.useSSL,
      apiVersion: props.settings.version,
    });
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
          this.setState({ isTestingSettings: false });

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
        });
    }
  }

  onValueChange() {
    this.props.onChange({
      client: this.state.client,
      hostname: this.state.hostname,
      baseUrl: this.state.baseUrl,
      port: this.state.port,
      apiKey: this.state.apiKey,
      apiUsername: this.state.apiUsername,
      useSSL: this.state.useSSL,
      qualityProfile: this.state.qualityProfile,
      path: this.state.path,
      profile: this.state.profile,
      version: this.state.apiVersion,
    });

    this.onValidate();
  }

  onValidate() {
    this.props.onValidate(this.state.isApiKeyValid && this.state.isHostnameValid && this.state.isPortValid);
  }

  render() {
    return (
      <>
        <div>
          <h6 className="heading-small text-muted mb-4">
            Ombi Connection Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <Dropdown
                name="API"
                value={this.state.apiVersion}
                items={[{ name: "Version 3-4", value: "3" }]}
                onChange={newApiVersion => this.setState({ apiVersion: newApiVersion }, this.onValueChange)} />
            </Col>
            <Col lg="6">
              <ValidatedTextbox
                name="API Key"
                placeholder="Enter api key"
                alertClassName="mt-3"
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
                placeholder="Enter base url configured in Ombi, leave empty if none configured."
                value={this.state.baseUrl}
                onChange={newBaseUrl => this.setState({ baseUrl: newBaseUrl }, this.onValueChange)} />
            </Col>
            <Col lg="6">
              <Textbox
                name="Default Ombi Username"
                placeholder="Enter api username (Optional)"
                value={this.state.apiUsername}
                onChange={newApiUsername => this.setState({ apiUsername: newApiUsername }, this.onValueChange)} />
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
            <Col lg="6">
              <a href="https://github.com/darkalfx/requestrr/wiki/Configuring-Ombi#configuring-permissions" target="_blank">Click here to view how configure Ombi permissions with the bot</a>
            </Col>
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
                        <Oval
                          wrapperClass="loader"
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
        <hr className="my-4" />
      </>
    );
  }
}

const mapPropsToAction = {
  testSettings: testOmbiSettings,
};

export default connect(null, mapPropsToAction)(Ombi);