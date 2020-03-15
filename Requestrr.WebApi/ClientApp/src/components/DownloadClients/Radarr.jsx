import React from "react";
import Loader from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { testRadarrSettings } from "../../store/actions/MovieClientsActions"
import { loadRadarrProfiles } from "../../store/actions/MovieClientsActions"
import { loadRadarrRootPaths } from "../../store/actions/MovieClientsActions"
import { loadRadarrTags } from "../../store/actions/MovieClientsActions"
import ValidatedTextbox from "../Inputs/ValidatedTextbox"
import Textbox from "../Inputs/Textbox"
import Dropdown from "../Inputs/Dropdown"
import MultiDropdown from "../Inputs/MultiDropdown"

import {
  FormGroup,
  Input,
  Row,
  Col
} from "reactstrap";

class Radarr extends React.Component {
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
      useSSL: "",
      isLoadingPaths: false,
      arePathsValid: true,
      moviePath: "",
      animePath: "",
      paths: [],
      isLoadingProfiles: false,
      areProfilesValid: true,
      movieProfile: 1,
      animeProfile: 1,
      profiles: [],
      isLoadingTags: false,
      areTagsValid: true,
      movieTags: 1,
      animeTags: 1,
      tags: [],
      movieMinAvailability: "announced",
      animeMinAvailability: "announced",
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
    this.onLoadProfiles = this.onLoadProfiles.bind(this);
    this.onLoadPaths = this.onLoadPaths.bind(this);
    this.onLoadTags = this.onLoadTags.bind(this);
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
      useSSL: props.settings.useSSL,
      moviePath: props.settings.moviePath,
      movieProfile: props.settings.movieProfile,
      movieMinAvailability: props.settings.movieMinAvailability,
      movieTags: props.settings.movieTags,
      animePath: props.settings.animePath,
      animeProfile: props.settings.animeProfile,
      animeMinAvailability: props.settings.animeMinAvailability,
      animeTags: props.settings.animeTags,
      apiVersion: props.settings.version,
      baseUrl: props.settings.baseUrl,
      searchNewRequests: props.settings.searchNewRequests,
      monitorNewRequests: props.settings.monitorNewRequests,
    }, () => {
      if (!this.validateNonEmptyString(this.state.movieMinAvailability)) {
        this.setState({ movieMinAvailability: "announced" });
      }

      if (!this.validateNonEmptyString(this.state.animeMinAvailability)) {
        this.setState({ animeMinAvailability: "announced" });
      }

      if (this.validateNonEmptyString(this.state.hostname) && this.validateNonEmptyString(this.state.apiKey) && this.validatePort(this.state.port)) {
        this.onLoadPaths();
        this.onLoadProfiles();

        if (this.state.apiVersion !== "2") {
          this.onLoadTags();
        }
      }
    });
  }

  onUseSSLChanged = event => {
    this.setState({
      useSSL: !this.state.useSSL
    }, this.onValueChange);
  }

  onLoadProfiles() {
    if (!this.state.isLoadingProfiles) {
      this.setState({ isLoadingProfiles: true });

      this.props.loadProfiles({
        hostname: this.state.hostname,
        baseUrl: this.state.baseUrl,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            var defaultProfileId = data.profiles.length > 0 ? data.profiles[0].id : 1;
            var movieProfileValue = data.profiles.map(x => x.id).includes(this.state.movieProfile) ? this.state.movieProfile : defaultProfileId;
            var animeProfileValue = data.profiles.map(x => x.id).includes(this.state.animeProfile) ? this.state.animeProfile : defaultProfileId;

            this.setState({
              movieProfile: movieProfileValue,
              animeProfile: animeProfileValue,
              profiles: data.profiles,
              areProfilesValid: true,
              isLoadingProfiles: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              movieProfile: 1,
              animeProfile: 1,
              profiles: [],
              areProfilesValid: false,
              isLoadingProfiles: false
            }, this.onValueChange);
          }
        });
    }
  }

  onLoadPaths() {
    if (!this.state.isLoadingPaths) {
      this.setState({ isLoadingPaths: true });

      this.props.loadRootPaths({
        hostname: this.state.hostname,
        baseUrl: this.state.baseUrl,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            var defaultPath = data.paths.length > 0 ? data.paths[0].path : "";
            var moviePathValue = data.paths.map(x => x.path).includes(this.state.moviePath) ? this.state.moviePath : defaultPath;
            var animePathValue = data.paths.map(x => x.path).includes(this.state.animePath) ? this.state.animePath : defaultPath;

            this.setState({
              moviePath: moviePathValue,
              animePath: animePathValue,
              paths: data.paths,
              arePathsValid: true,
              isLoadingPaths: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              moviePath: "",
              animePath: "",
              paths: [],
              arePathsValid: false,
              isLoadingPaths: false
            }, this.onValueChange);
          }
        });
    }
  }

  onLoadTags() {
    if (!this.state.isLoadingTags) {
      this.setState({ isLoadingTags: true });

      this.props.loadTags({
        hostname: this.state.hostname,
        baseUrl: this.state.baseUrl,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            var movieTagsValue = this.state.movieTags.filter(x => data.tags.map(x => x.id).includes(x));
            var animeTagsValue = this.state.animeTags.filter(x => data.tags.map(x => x.id).includes(x));

            this.setState({
              movieTags: movieTagsValue,
              animeTags: animeTagsValue,
              tags: data.tags,
              areTagsValid: true,
              isLoadingTags: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              movieTags: [],
              animeTags: [],
              tags: [],
              areTagsValid: false,
              isLoadingTags: false
            }, this.onValueChange);
          }
        });
    }
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
    this.props.onChange({
      client: this.state.client,
      hostname: this.state.hostname,
      baseUrl: this.state.baseUrl,
      port: this.state.port,
      apiKey: this.state.apiKey,
      useSSL: this.state.useSSL,
      moviePath: this.state.moviePath,
      movieProfile: this.state.movieProfile,
      movieMinAvailability: this.state.movieMinAvailability,
      movieTags: this.state.movieTags,
      animePath: this.state.animePath,
      animeProfile: this.state.animeProfile,
      animeMinAvailability: this.state.animeMinAvailability,
      animeTags: this.state.animeTags,
      version: this.state.apiVersion,
      searchNewRequests: this.state.searchNewRequests,
      monitorNewRequests: this.state.monitorNewRequests,
    });

    this.onValidate();
  }

  onValidate() {
    this.props.onValidate(
      this.state.isApiKeyValid
      && this.state.isHostnameValid
      && this.state.isPortValid
      && this.state.profiles.length > 0
      && this.state.paths.length > 0);
  }

  render() {
    return (
      <>
        <div>
          <h6 className="heading-small text-muted mb-4">
            Radarr Connection Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <Dropdown
                name="Api"
                value={this.state.apiVersion}
                items={[{ name: "Version 2", value: "2" }, { name: "Version 3", value: "3" }]}
                onChange={newApiVersion => this.setState({ apiVersion: newApiVersion }, this.onValueChange)} />
            </Col>
            <Col lg="6">
              <ValidatedTextbox
                name="Api Key"
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
                name="Hostname"
                placeholder="Enter hostname"
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
                placeholder="Enter base url configured in Radarr, leave empty if none configured."
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
            <Col lg="6">
            </Col>
          </Row>
        </div>
        <div>
          <h6 className="heading-small text-muted mt-4">
            Radarr Movie Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6" className="mb-4">
              <div className="input-group-select-box">
                <Dropdown
                  name="Path"
                  value={this.state.moviePath}
                  items={this.state.paths.map(x => { return { name: x.path, value: x.path } })}
                  onChange={newPath => this.setState({ moviePath: newPath }, this.onValueChange)} />
                <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadPaths} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                  <span className="btn-inner--icon">
                    {
                      this.state.isLoadingPaths ? (
                        <Loader
                          className="loader"
                          type="Oval"
                          color="#11cdef"
                          height={19}
                          width={19}
                        />)
                        : (<i className="fas fa-download"></i>)
                    }</span>
                  <span className="btn-inner--text">Load</span>
                </button>
              </div>
              {
                !this.state.arePathsValid ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>Could not load paths, cannot reach Radarr.</strong>
                  </Alert>)
                  : null
              }
              {
                this.props.isSubmitted && this.state.paths.length === 0 ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>A path is required.</strong>
                  </Alert>)
                  : null
              }
            </Col>
            <Col lg="6" className="mb-4">
              <div className="input-group-select-box">
                <Dropdown
                  name="Profile"
                  value={this.state.movieProfile}
                  items={this.state.profiles.map(x => { return { name: x.name, value: x.id } })}
                  onChange={newProfile => this.setState({ movieProfile: newProfile }, this.onValueChange)} />
                <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadProfiles} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                  <span className="btn-inner--icon">
                    {
                      this.state.isLoadingProfiles ? (
                        <Loader
                          className="loader"
                          type="Oval"
                          color="#11cdef"
                          height={19}
                          width={19}
                        />)
                        : (<i className="fas fa-download"></i>)
                    }</span>
                  <span className="btn-inner--text">Load</span>
                </button>
              </div>
              {
                !this.state.areProfilesValid ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>Could not load profiles, cannot reach Radarr.</strong>
                  </Alert>)
                  : null
              }
              {
                this.props.isSubmitted && this.state.profiles.length === 0 ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>A profile is required.</strong>
                  </Alert>)
                  : null
              }
            </Col>
          </Row>
          <Row>
            <Col lg="6">
              <Dropdown
                name="Min Availability"
                value={this.state.movieMinAvailability}
                items={[{ name: "Announced", value: "announced" }, { name: "In Cinemas", value: "inCinemas" }, { name: "Physical/Web", value: "released" }, { name: "PreDB", value: "preDB" }]}
                onChange={newMinAvailability => this.setState({ movieMinAvailability: newMinAvailability }, this.onValueChange)} />
            </Col>
            <Col lg="6">
              {
                this.state.apiVersion !== "2"
                  ? <>
                    <div className="input-group-select-box">
                      <MultiDropdown
                        name="Tags"
                        placeholder=""
                        labelField="name"
                        valueField="id"
                        selectedItems={this.state.tags.filter(x => this.state.movieTags.includes(x.id))}
                        items={this.state.tags}
                        onChange={newMovieTags => this.setState({ movieTags: newMovieTags.map(x => x.id) }, this.onValueChange)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadTags} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.state.isLoadingTags ? (
                              <Loader
                                className="loader"
                                type="Oval"
                                color="#11cdef"
                                height={19}
                                width={19}
                              />)
                              : (<i className="fas fa-download"></i>)
                          }</span>
                        <span className="btn-inner--text">Load</span>
                      </button>
                    </div>
                    {
                      !this.state.areTagsValid ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>Could not load tags, cannot reach Radarr.</strong>
                        </Alert>)
                        : null
                    }
                  </>
                  : null
              }
            </Col>
          </Row>
        </div>
        <div>
          <h6 className="heading-small text-muted mt-4">
            Radarr Anime Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6" className="mb-4">
              <div className="input-group-select-box">
                <Dropdown
                  name="Path"
                  value={this.state.animePath}
                  items={this.state.paths.map(x => { return { name: x.path, value: x.path } })}
                  onChange={newPath => this.setState({ animePath: newPath }, this.onValueChange)} />
                <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadPaths} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                  <span className="btn-inner--icon">
                    {
                      this.state.isLoadingPaths ? (
                        <Loader
                          className="loader"
                          type="Oval"
                          color="#11cdef"
                          height={19}
                          width={19}
                        />)
                        : (<i className="fas fa-download"></i>)
                    }</span>
                  <span className="btn-inner--text">Load</span>
                </button>
              </div>
              {
                !this.state.arePathsValid ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>Could not load paths, cannot reach Radarr.</strong>
                  </Alert>)
                  : null
              }
              {
                this.props.isSubmitted && this.state.paths.length === 0 ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>A path is required.</strong>
                  </Alert>)
                  : null
              }
            </Col>
            <Col lg="6" className="mb-4">
              <div className="input-group-select-box">
                <Dropdown
                  name="Profile"
                  value={this.state.animeProfile}
                  items={this.state.profiles.map(x => { return { name: x.name, value: x.id } })}
                  onChange={newProfile => this.setState({ animeProfile: newProfile }, this.onValueChange)} />
                <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadProfiles} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                  <span className="btn-inner--icon">
                    {
                      this.state.isLoadingProfiles ? (
                        <Loader
                          className="loader"
                          type="Oval"
                          color="#11cdef"
                          height={19}
                          width={19}
                        />)
                        : (<i className="fas fa-download"></i>)
                    }</span>
                  <span className="btn-inner--text">Load</span>
                </button>
              </div>
              {
                !this.state.areProfilesValid ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>Could not load profiles, cannot reach Radarr.</strong>
                  </Alert>)
                  : null
              }
              {
                this.props.isSubmitted && this.state.profiles.length === 0 ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>A profile is required.</strong>
                  </Alert>)
                  : null
              }
            </Col>
          </Row>
          <Row className="mb-4">
            <Col lg="6">
              <Dropdown
                name="Min Availability"
                value={this.state.animeMinAvailability}
                items={[{ name: "Announced", value: "announced" }, { name: "In Cinemas", value: "inCinemas" }, { name: "Physical/Web", value: "released" }, { name: "PreDB", value: "preDB" }]}
                onChange={newMinAvailability => this.setState({ animeMinAvailability: newMinAvailability }, this.onValueChange)} />
            </Col>
            <Col lg="6">
              {
                this.state.apiVersion !== "2"
                  ? <>
                    <div className="input-group-select-box">
                      <MultiDropdown
                        name="Tags"
                        placeholder=""
                        labelField="name"
                        valueField="id"
                        selectedItems={this.state.tags.filter(x => this.state.animeTags.includes(x.id))}
                        items={this.state.tags}
                        onChange={newAnimeTags => this.setState({ animeTags: newAnimeTags.map(x => x.id) }, this.onValueChange)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadTags} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.state.isLoadingTags ? (
                              <Loader
                                className="loader"
                                type="Oval"
                                color="#11cdef"
                                height={19}
                                width={19}
                              />)
                              : (<i className="fas fa-download"></i>)
                          }</span>
                        <span className="btn-inner--text">Load</span>
                      </button>
                    </div>
                    {
                      !this.state.areTagsValid ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>Could not load tags, cannot reach Radarr.</strong>
                        </Alert>)
                        : null
                    }
                  </>
                  : null
              }
            </Col>
          </Row>
        </div>
        <div>
          <h6 className="heading-small text-muted mt-4">
            Radarr Requests Permissions Settings
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
                  <span className="text-muted">Automatically monitor newly added movies</span>
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
                  <span className="text-muted">Automatically search for movie when request is made</span>
                </label>
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col>
              <FormGroup className="mt-4">
                {
                  this.state.testSettingsRequested && !this.state.isTestingSettings ?
                    !this.state.testSettingsSuccess ? (
                      <Alert className="text-center" color="danger">
                        <strong>{this.state.testSettingsError}.</strong>
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
      </>
    );
  }
}

const mapPropsToAction = {
  testSettings: testRadarrSettings,
  loadProfiles: loadRadarrProfiles,
  loadRootPaths: loadRadarrRootPaths,
  loadTags: loadRadarrTags,
};

export default connect(null, mapPropsToAction)(Radarr);