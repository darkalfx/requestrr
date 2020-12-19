import React from "react";
import Loader from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { testLidarrSettings } from "../../store/actions/MusicClientsActions"
import { loadLidarrProfiles } from "../../store/actions/MusicClientsActions"
import { loadLidarrMetadataProfiles } from "../../store/actions/MusicClientsActions"
import { loadLidarrRootPaths } from "../../store/actions/MusicClientsActions"
import { loadLidarrTags } from "../../store/actions/MusicClientsActions"
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

class Lidarr extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isTestingSettings: false,
      testSettingsRequested: false,
      testSettingsSuccess: false,
      testSettingsError: "",
      hostname: "",
      isHostnameValid: false,
      port: "8686",
      isPortValid: false,
      apiKey: "",
      isApiKeyValid: false,
      useSSL: false,
      musicUseAlbumFolders: false,
      isLoadingPaths: false,
      arePathsValid: true,
      musicPath: "",
      paths: [],
      isLoadingProfiles: false,
      areProfilesValid: true,
      musicProfile: 1,
      profiles: [],
      isLoadingMetadataProfiles: false,
      areMetadataProfilesValid: true,
      musicMetadataProfile: 1,
      metadataProfiles: [],
      isLoadingTags: false,
      areTagsValid: true,
      musicTags: [],
      tags: [],
      apiVersion: "",
      baseUrl: "",
      searchNewRequests: true,
      monitorNewRequests: true,
    };

    this.onTestSettings = this.onTestSettings.bind(this);
    this.onUseSSLChanged = this.onUseSSLChanged.bind(this);
    this.onMusicUseAlbumFoldersChanged = this.onMusicUseAlbumFoldersChanged.bind(this);
    this.onValueChange = this.onValueChange.bind(this);
    this.onValidate = this.onValidate.bind(this);
    this.updateStateFromProps = this.updateStateFromProps.bind(this);
    this.validateNonEmptyString = this.validateNonEmptyString.bind(this);
    this.validatePort = this.validatePort.bind(this);
    this.onLoadProfiles = this.onLoadProfiles.bind(this);
    this.onLoadMetadataProfiles = this.onLoadMetadataProfiles.bind(this);
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
      musicPath: props.settings.musicPath,
      musicProfile: props.settings.musicProfile,
      musicMetadataProfile: props.settings.musicMetadataProfile,
      musicTags: props.settings.musicTags,
      musicUseAlbumFolders: props.settings.musicUseAlbumFolders,
      baseUrl: props.settings.baseUrl,
      searchNewRequests: props.settings.searchNewRequests,
      monitorNewRequests: props.settings.monitorNewRequests,
    }, () => {
      if (this.validateNonEmptyString(this.state.hostname) && this.validateNonEmptyString(this.state.apiKey) && this.validatePort(this.state.port)) {
        this.onLoadPaths();
        this.onLoadProfiles();
        this.onLoadMetadataProfiles();
        this.onLoadTags();
      }
    });
  }

  onUseSSLChanged = event => {
    this.setState({
      useSSL: !this.state.useSSL
    }, this.onValueChange);
  }

  onMusicUseAlbumFoldersChanged = event => {
    this.setState({
      musicUseAlbumFolders: !this.state.musicUseAlbumFolders
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
      })
        .then(data => {
          if (data.ok) {
            var defaultProfileId = data.profiles.length > 0 ? data.profiles[0].id : 1;
            var musicProfileValue = data.profiles.map(x => x.id).includes(this.state.musicProfile) ? this.state.musicProfile : defaultProfileId;

            this.setState({
              musicProfile: musicProfileValue,
              profiles: data.profiles,
              areProfilesValid: true,
              isLoadingProfiles: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              musicProfile: 1,
              profiles: [],
              areProfilesValid: false,
              isLoadingProfiles: false
            }, this.onValueChange);
          }
        });
    }
  }

  onLoadMetadataProfiles() {
    if (!this.state.isLoadingMetadataProfiles) {
      this.setState({ isLoadingMetadataProfiles: true });

      this.props.loadMetadataProfiles({
        hostname: this.state.hostname,
        baseUrl: this.state.baseUrl,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
      })
        .then(data => {
          if (data.ok) {
            var defaultMetadataProfileId = data.metadataProfiles.length > 0 ? data.metadataProfiles[0].id : 1;
            var musicMetadataProfileValue = data.metadataProfiles.map(x => x.id).includes(this.state.musicMetadataProfile) ? this.state.musicMetadataProfile : defaultMetadataProfileId;

            this.setState({
              musicMetadataProfile: musicMetadataProfileValue,
              metadataProfiles: data.metadataProfiles,
              areMetadataProfilesValid: true,
              isLoadingMetadataProfiles: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              musicMetadataProfile: 1,
              metadataProfiles: [],
              areMetadataProfilesValid: false,
              isLoadingMetadataProfiles: false
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
      })
        .then(data => {
          if (data.ok) {
            var defaultPath = data.paths.length > 0 ? data.paths[0].path : "";
            var musicPathValue = data.paths.map(x => x.path).includes(this.state.musicPath) ? this.state.musicPath : defaultPath;

            this.setState({
              musicPath: musicPathValue,
              paths: data.paths,
              arePathsValid: true,
              isLoadingPaths: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              musicPath: "",
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
        useSSL: this.state.useSSL
      })
        .then(data => {
          if (data.ok) {
            var musicTagsValue = this.state.musicTags.filter(x => data.tags.map(x => x.id).includes(x));

            this.setState({
              musicTags: musicTagsValue,
              tags: data.tags,
              areTagsValid: true,
              isLoadingTags: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              musicTags: [],
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
        useSSL: this.state.useSSL
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
      hostname: this.state.hostname,
      baseUrl: this.state.baseUrl,
      port: this.state.port,
      apiKey: this.state.apiKey,
      musicPath: this.state.musicPath,
      musicProfile: this.state.musicProfile,
      musicMetadataProfile: this.state.musicMetadataProfile,
      musicTags: this.state.musicTags,
      musicUseAlbumFolders: this.state.musicUseAlbumFolders,
      useSSL: this.state.useSSL,
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
      && this.state.metadataProfiles.length > 0
      && this.state.paths.length > 0);
  }

  render() {
    return (
      <>
        <div>
          <h6 className="heading-small text-muted mb-4">
            Lidarr Connection Settings
        </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
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
                placeholder="Enter base url configured in Lidarr, leave empty if none configured."
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
        </div>
        <div>
          <h6 className="heading-small text-muted mt-4">
            Lidarr Artist Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <div className="input-group-button mt-4">
                <Dropdown
                  name="Path"
                  value={this.state.musicPath}
                  items={this.state.paths.map(x => { return { name: x.path, value: x.path } })}
                  onChange={newPath => this.setState({ musicPath: newPath }, this.onValueChange)} />
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
                    <strong>Could not load paths, cannot reach Lidarr.</strong>
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
            <Col lg="6">
              <div className="input-group-button mt-4">
                <Dropdown
                  name="Profile"
                  value={this.state.musicProfile}
                  items={this.state.profiles.map(x => { return { name: x.name, value: x.id } })}
                  onChange={newProfile => this.setState({ musicProfile: newProfile }, this.onValueChange)} />
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
                    <strong>Could not load profiles, cannot reach Lidarr.</strong>
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
          {
            this.state.apiVersion !== "2"
              ? <>
                <Row>
                  <Col lg="6">
                    <div className="input-group-button mt-4">
                      <Dropdown
                        name="Metadata Profile"
                        value={this.state.musicMetadataProfile}
                        items={this.state.metadataProfiles.map(x => { return { name: x.name, value: x.id } })}
                        onChange={newMetadataProfile => this.setState({ musicMetadataProfile: newMetadataProfile }, this.onValueChange)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadMetadataProfiles} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.state.isLoadingMetadataProfiles ? (
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
                      !this.state.areMetadataProfilesValid ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>Could not load metadata profiles, cannot reach Lidarr.</strong>
                        </Alert>)
                        : null
                    }
                    {
                      this.props.isSubmitted && this.state.metadataProfiles.length === 0 ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>A metadata profile is required.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                  <Col lg="6">
                    <div className="input-group-button mt-4">
                      <MultiDropdown
                        name="Tags"
                        placeholder=""
                        labelField="name"
                        valueField="id"
                        selectedItems={this.state.tags.filter(x => this.state.musicTags.includes(x.id))}
                        items={this.state.tags}
                        onChange={newTvTags => this.setState({ musicTags: newTvTags.map(x => x.id) }, this.onValueChange)} />
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
                          <strong>Could not load tags, cannot reach Lidarr.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                </Row>
              </>
              : null
          }
          <Row>
            <Col className="mt-4" lg="6">
              <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                <Input
                  className="custom-control-input"
                  id="musicUseAlbumFolders"
                  type="checkbox"
                  onChange={this.onMusicUseAlbumFoldersChanged}
                  checked={this.state.musicUseAlbumFolders}
                />
                <label
                  className="custom-control-label"
                  htmlFor="musicUseAlbumFolders">
                  <span className="text-muted">Use album folders</span>
                </label>
              </FormGroup>
            </Col>
          </Row>
        </div>
        <div>
          <h6 className="heading-small text-muted mt-4">
            Lidarr Requests Permissions Settings
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
                  <span className="text-muted">Automatically monitor newly added artists</span>
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
                  <span className="text-muted">Automatically search for albums when a request is made</span>
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
      </>
    );
  }
}

const mapPropsToAction = {
  testSettings: testLidarrSettings,
  loadProfiles: loadLidarrProfiles,
  loadMetadataProfiles: loadLidarrMetadataProfiles,
  loadRootPaths: loadLidarrRootPaths,
  loadTags: loadLidarrTags,
};

export default connect(null, mapPropsToAction)(Lidarr);