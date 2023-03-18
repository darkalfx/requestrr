import React from "react";
import { Oval } from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { testOverseerrMovieSettings } from "../../../../store/actions/OverseerrClientRadarrActions"
import { setOverseerrMovieConnectionSettings } from "../../../../store/actions/OverseerrClientRadarrActions"
import ValidatedTextbox from "../../../Inputs/ValidatedTextbox"
import Dropdown from "../../../Inputs/Dropdown"
import OverseerrMovieCategoryList from "./OverseerrMovieCategoryList"

import {
  FormGroup,
  Input,
  Row,
  Col
} from "reactstrap";

class OverseerrMovie extends React.Component {
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
      defaultApiUserID: "",
      isDefaultApiUserIDValid: true,
      useSSL: "",
      apiVersion: "",
    };

    this.onTestSettings = this.onTestSettings.bind(this);
    this.onUseSSLChanged = this.onUseSSLChanged.bind(this);
    this.onValueChange = this.onValueChange.bind(this);
    this.onValidate = this.onValidate.bind(this);
    this.updateStateFromProps = this.updateStateFromProps.bind(this);
    this.validateNonEmptyString = this.validateNonEmptyString.bind(this);
    this.validatePort = this.validatePort.bind(this);
    this.validateDefaultUserId = this.validateDefaultUserId.bind(this);
  }

  componentDidMount() {
    this.updateStateFromProps(this.props);
  }

  componentDidUpdate(prevProps) {
    this.onValidate();
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
      defaultApiUserID: props.settings.defaultApiUserID,
      isDefaultApiUserIDValid: true,
      useSSL: props.settings.useSSL,
      apiVersion: props.settings.version,
      isValid: false,
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

  validateDefaultUserId = value => {
    return (!value || value.length === 0 || /^\s*$/.test(value)) || (/^[1-9]\d*$/).test(value);
  }

  onTestSettings = e => {
    e.preventDefault();

    if (!this.state.isTestingSettings
      && this.state.isHostnameValid
      && this.state.isPortValid
      && this.state.isDefaultApiUserIDValid
      && this.state.isApiKeyValid) {
      this.setState({ isTestingSettings: true });

      this.props.testSettings({
        hostname: this.state.hostname,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        DefaultApiUserID: this.state.DefaultApiUserID,
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
    this.props.setConnectionSettings({
      hostname: this.state.hostname,
      port: this.state.port,
      apiKey: this.state.apiKey,
      useSSL: this.state.useSSL,
      version: this.state.apiVersion,
    });

    this.props.onChange({
      client: this.state.client,
      hostname: this.state.hostname,
      port: this.state.port,
      apiKey: this.state.apiKey,
      defaultApiUserID: this.state.defaultApiUserID,
      useSSL: this.state.useSSL,
      version: this.state.apiVersion,
    });

    this.onValidate();
  }

  onValidate() {
    let isValid = this.state.isApiKeyValid
      && this.state.isHostnameValid
      && this.state.isPortValid
      && this.state.isDefaultApiUserIDValid
      && (this.props.settings.categories.length === 0 || (this.props.settings.categories.every(x => this.validateCategory(x)) && this.props.settings.isRadarrServiceSettingsValid));

    if (isValid !== this.state.isValid) {
      this.setState({ isValid: isValid },
        () => this.props.onValidate(isValid));
    }
  }

  validateCategory(category) {
    if (!/\S/.test(category.name)) {
      return false;
    }
    else if (/^[\w-]{1,32}$/.test(category.name)) {
      var names = this.props.settings.categories.map(x => x.name);

      if (new Set(names).size !== names.length) {
        return false;
      }
      else if (this.props.settings.radarrServiceSettings.radarrServices.every(x => x.id !== category.serviceId)) {
        return false;
      }
      else {
        var radarrService = this.props.settings.radarrServiceSettings.radarrServices.filter(x => x.id === category.serviceId)[0];

        if (radarrService.profiles.length === 0 || radarrService.rootPaths.length === 0) {
          return false;
        }
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
            Overseerr Connection Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <Dropdown
                name="API"
                value={this.state.apiVersion}
                items={[{ name: "Version 1", value: "1" }]}
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
              <ValidatedTextbox
                name="Default Overseerr User ID for requests"
                placeholder="Enter default user ID (Optional)"
                alertClassName="mt-3 mb-0"
                errorMessage="The user id must be a number."
                isSubmitted={this.props.isSubmitted}
                value={this.state.defaultApiUserID}
                validation={this.validateDefaultUserId}
                onChange={newDefaultApiUserID => this.setState({ defaultApiUserID: newDefaultApiUserID }, this.onValueChange)}
                onValidate={isValid => this.setState({ isDefaultApiUserIDValid: isValid }, this.onValidate)} />
            </Col>
            <Col lg="6"></Col>
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
              <a href="https://github.com/darkalfx/requestrr/wiki/Configuring-Overseerr#configuring-permissions" target="_blank">Click here to view how configure Overseerr permissions with the bot</a>
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
                <button onClick={this.onTestSettings} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid || !this.state.isDefaultApiUserIDValid} className="btn btn-icon btn-3 btn-default" type="button">
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
        <OverseerrMovieCategoryList isSubmitted={this.props.isSubmitted} isSaving={this.props.isSaving} apiVersion={this.state.apiVersion} canConnect={this.state.isHostnameValid && this.state.isPortValid && this.state.isApiKeyValid} />
      </>
    );
  }
}

const mapPropsToState = state => {
  return {
    settings: state.movies.overseerr
  }
};

const mapPropsToAction = {
  testSettings: testOverseerrMovieSettings,
  setConnectionSettings: setOverseerrMovieConnectionSettings,
};

export default connect(mapPropsToState, mapPropsToAction)(OverseerrMovie);