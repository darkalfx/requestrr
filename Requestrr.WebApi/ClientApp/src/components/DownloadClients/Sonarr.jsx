import React from "react";
import Loader from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { testSonarrSettings } from "../../store/actions/TvShowsClientsActions"
import { loadSonarrProfiles } from "../../store/actions/TvShowsClientsActions"
import { loadSonarrRootPaths } from "../../store/actions/TvShowsClientsActions"
import { loadSonarrLanguages } from "../../store/actions/TvShowsClientsActions"
import { loadSonarrTags } from "../../store/actions/TvShowsClientsActions"
import ValidatedTextbox from "../Inputs/ValidatedTextbox"
import Dropdown from "../Inputs/Dropdown"
import MultiDropdown from "../Inputs/MultiDropdown"

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
      tvUseSeasonFolders: false,
      animeUseSeasonFolders: false,
      isLoadingPaths: false,
      arePathsValid: true,
      tvPath: "",
      animePath: "",
      paths: [],
      isLoadingProfiles: false,
      areProfilesValid: true,
      tvProfile: 1,
      animeProfile: 1,
      profiles: [],
      isLoadingLanguages: false,
      areLanguageValid: true,
      tvLanguage: 1,
      animeLanguage: 1,
      languages: [],
      isLoadingTags: false,
      areTagsValid: true,
      tvTags: [],
      animeTags: [],
      tags: [],
      apiVersion: ""
    };

    this.onTestSettings = this.onTestSettings.bind(this);
    this.onUseSSLChanged = this.onUseSSLChanged.bind(this);
    this.onTvUseSeasonFoldersChanged = this.onTvUseSeasonFoldersChanged.bind(this);
    this.onAnimeUseSeasonFoldersChanged = this.onAnimeUseSeasonFoldersChanged.bind(this);
    this.onValueChange = this.onValueChange.bind(this);
    this.onValidate = this.onValidate.bind(this);
    this.updateStateFromProps = this.updateStateFromProps.bind(this);
    this.validateNonEmptyString = this.validateNonEmptyString.bind(this);
    this.validatePort = this.validatePort.bind(this);
    this.onLoadProfiles = this.onLoadProfiles.bind(this);
    this.onLoadPaths = this.onLoadPaths.bind(this);
    this.onLoadLanguages = this.onLoadLanguages.bind(this);
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
      tvPath: props.settings.tvPath,
      tvProfile: props.settings.tvProfile,
      tvTags: props.settings.tvTags,
      tvLanguage: props.settings.tvLanguage,
      tvUseSeasonFolders: props.settings.tvUseSeasonFolders,
      animePath: props.settings.animePath,
      animeProfile: props.settings.animeProfile,
      animeTags: props.settings.animeTags,
      animeLanguage: props.settings.animeLanguage,
      animeUseSeasonFolders: props.settings.animeUseSeasonFolders,
      apiVersion: props.settings.version
    }, () => {
      if (this.validateNonEmptyString(this.state.hostname) && this.validateNonEmptyString(this.state.apiKey) && this.validatePort(this.state.port)) {
        this.onLoadPaths();
        this.onLoadProfiles();

        if (this.state.apiVersion !== "2") {
          this.onLoadLanguages();
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

  onTvUseSeasonFoldersChanged = event => {
    this.setState({
      tvUseSeasonFolders: !this.state.tvUseSeasonFolders
    }, this.onValueChange);
  }

  onAnimeUseSeasonFoldersChanged = event => {
    this.setState({
      animeUseSeasonFolders: !this.state.animeUseSeasonFolders
    }, this.onValueChange);
  }

  onLoadProfiles() {
    if (!this.state.isLoadingProfiles) {
      this.setState({ isLoadingProfiles: true });

      this.props.loadProfiles({
        hostname: this.state.hostname,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            var defaultProfileId = data.profiles.length > 0 ? data.profiles[0].id : 1;
            var tvProfileValue = data.profiles.map(x => x.id).includes(this.state.tvProfile) ? this.state.tvProfile : defaultProfileId;
            var animeProfileValue = data.profiles.map(x => x.id).includes(this.state.animeProfile) ? this.state.animeProfile : defaultProfileId;

            this.setState({
              tvProfile: tvProfileValue,
              animeProfile: animeProfileValue,
              profiles: data.profiles,
              areProfilesValid: true,
              isLoadingProfiles: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              tvProfile: 1,
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
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            var defaultPath = data.paths.length > 0 ? data.paths[0].path : "";
            var tvPathValue = data.paths.map(x => x.path).includes(this.state.tvPath) ? this.state.tvPath : defaultPath;
            var animePathValue = data.paths.map(x => x.path).includes(this.state.animePath) ? this.state.animePath : defaultPath;

            this.setState({
              tvPath: tvPathValue,
              animePath: animePathValue,
              paths: data.paths,
              arePathsValid: true,
              isLoadingPaths: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              tvPath: "",
              animePath: "",
              paths: [],
              arePathsValid: false,
              isLoadingPaths: false
            }, this.onValueChange);
          }
        });
    }
  }

  onLoadLanguages() {
    if (!this.state.isLoadingLanguages) {
      this.setState({ isLoadingLanguages: true });

      this.props.loadLanguages({
        hostname: this.state.hostname,
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            var defaultLanguageId = data.languages.length > 0 ? data.languages[0].id : 1;
            var tvLanguageValue = data.languages.map(x => x.id).includes(this.state.tvLanguage) ? this.state.tvLanguage : defaultLanguageId;
            var animeLanguageValue = data.languages.map(x => x.id).includes(this.state.animeLanguage) ? this.state.animeLanguage : defaultLanguageId;

            this.setState({
              tvLanguage: tvLanguageValue,
              animeLanguage: animeLanguageValue,
              languages: data.languages,
              areLanguageValid: true,
              isLoadingLanguages: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              tvLanguage: 1,
              animeLanguage: 1,
              languages: [],
              areLanguageValid: false,
              isLoadingLanguages: false
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
        port: this.state.port,
        apiKey: this.state.apiKey,
        useSSL: this.state.useSSL,
        version: this.state.apiVersion,
      })
        .then(data => {
          if (data.ok) {
            var tvTagsValue = this.state.tvTags.filter(x => data.tags.map(x => x.id).includes(x));
            var animeTagsValue = this.state.animeTags.filter(x => data.tags.map(x => x.id).includes(x));

            this.setState({
              tvTags: tvTagsValue,
              animeTags: animeTagsValue,
              tags: data.tags,
              areTagsValid: true,
              isLoadingTags: false
            }, this.onValueChange);
          }
          else {
            this.setState({
              tvTags: [],
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
      hostname: this.state.hostname,
      port: this.state.port,
      apiKey: this.state.apiKey,
      tvPath: this.state.tvPath,
      tvProfile: this.state.tvProfile,
      tvTags: this.state.tvTags,
      tvLanguage: this.state.tvLanguage,
      tvUseSeasonFolders: this.state.tvUseSeasonFolders,
      animePath: this.state.animePath,
      animeProfile: this.state.animeProfile,
      animeTags: this.state.animeTags,
      animeLanguage: this.state.animeLanguage,
      animeUseSeasonFolders: this.state.animeUseSeasonFolders,
      useSSL: this.state.useSSL,
      version: this.state.apiVersion,
    });

    this.onValidate();
  }

  onValidate() {
    var isLanguageValid = this.state.apiVersion !== "2" ? this.state.languages.length > 0 : true;

    this.props.onValidate(
      this.state.isApiKeyValid
      && this.state.isHostnameValid
      && this.state.isPortValid
      && this.state.profiles.length > 0
      && this.state.paths.length > 0
      && isLanguageValid);
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
            Sonarr Tv Show Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <div className="input-group-select-box mt-4">
                <Dropdown
                  name="Path"
                  value={this.state.tvPath}
                  items={this.state.paths.map(x => { return { name: x.path, value: x.path } })}
                  onChange={newPath => this.setState({ tvPath: newPath }, this.onValueChange)} />
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
                    <strong>Could not load paths, cannot reach Sonarr.</strong>
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
              <div className="input-group-select-box mt-4">
                <Dropdown
                  name="Profile"
                  value={this.state.tvProfile}
                  items={this.state.profiles.map(x => { return { name: x.name, value: x.id } })}
                  onChange={newProfile => this.setState({ tvProfile: newProfile }, this.onValueChange)} />
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
                    <strong>Could not load profiles, cannot reach Sonarr.</strong>
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
                    <div className="input-group-select-box mt-4">
                      <MultiDropdown
                        name="Tags"
                        placeholder=""
                        labelField="name"
                        valueField="id"
                        selectedItems={this.state.tags.filter(x => this.state.tvTags.includes(x.id))}
                        items={this.state.tags}
                        onChange={newTvTags => this.setState({ tvTags: newTvTags.map(x => x.id) }, this.onValueChange)} />
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
                          <strong>Could not load tags, cannot reach Sonarr.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                  <Col lg="6">
                    <div className="input-group-select-box mt-4">
                      <Dropdown
                        name="Language"
                        value={this.state.tvLanguage}
                        items={this.state.languages.map(x => { return { name: x.name, value: x.id } })}
                        onChange={newLanguage => this.setState({ tvLanguage: newLanguage }, this.onValueChange)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadLanguages} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.state.isLoadingLanguages ? (
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
                      !this.state.areLanguageValid ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>Could not load languages, cannot reach Sonarr.</strong>
                        </Alert>)
                        : null
                    }
                    {
                      this.props.isSubmitted && this.state.languages.length === 0 ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>A language is required.</strong>
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
                  id="tvUseSeasonFolders"
                  type="checkbox"
                  onChange={this.onTvUseSeasonFoldersChanged}
                  checked={this.state.tvUseSeasonFolders}
                />
                <label
                  className="custom-control-label"
                  htmlFor="tvUseSeasonFolders">
                  <span className="text-muted">Use season folders</span>
                </label>
              </FormGroup>
            </Col>
          </Row>
        </div>
        <div>
          <h6 className="heading-small text-muted mt-4">
            Sonarr Anime Settings
          </h6>
        </div>
        <div className="pl-lg-4">
          <Row>
            <Col lg="6">
              <div className="input-group-select-box mt-4">
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
                    <strong>Could not load paths, cannot reach Sonarr.</strong>
                  </Alert>)
                  : null
              }
              {
                this.props.isSubmitted && this.state.paths.length === 0 ? (
                  <Alert className="mt-3 mb-0" color="warning">
                    <strong>An path is required.</strong>
                  </Alert>)
                  : null
              }
            </Col>
            <Col lg="6">
              <div className="input-group-select-box mt-4">
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
                    <strong>Could not load profiles, cannot reach Sonarr.</strong>
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
                    <div className="input-group-select-box mt-4">
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
                          <strong>Could not load tags, cannot reach Sonarr.</strong>
                        </Alert>)
                        : null
                    }
                  </Col>
                  <Col lg="6">
                    <div className="input-group-select-box mt-4">
                      <Dropdown
                        name="Language"
                        value={this.state.animeLanguage}
                        items={this.state.languages.map(x => { return { name: x.name, value: x.id } })}
                        onChange={newLanguage => this.setState({ animeLanguage: newLanguage }, this.onValueChange)} />
                      <button className="btn btn-icon btn-3 btn-default" onClick={this.onLoadLanguages} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} type="button">
                        <span className="btn-inner--icon">
                          {
                            this.state.isLoadingLanguages ? (
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
                      !this.state.areLanguageValid ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>Could not load languages, cannot reach Sonarr.</strong>
                        </Alert>)
                        : null
                    }
                    {
                      this.props.isSubmitted && this.state.languages.length === 0 ? (
                        <Alert className="mt-3 mb-0" color="warning">
                          <strong>A language is required.</strong>
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
                  id="animeUseSeasonFolders"
                  type="checkbox"
                  onChange={this.onAnimeUseSeasonFoldersChanged}
                  checked={this.state.animeUseSeasonFolders}
                />
                <label
                  className="custom-control-label"
                  htmlFor="animeUseSeasonFolders">
                  <span className="text-muted">Use season folders</span>
                </label>
              </FormGroup>
            </Col>
          </Row>
          <Row>
            <Col lg="6">

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
                <button onClick={this.onTestSettings} disabled={!this.state.isHostnameValid || !this.state.isPortValid || !this.state.isApiKeyValid} className="btn btn-icon btn-3 btn-default" type="submit">
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
  testSettings: testSonarrSettings,
  loadProfiles: loadSonarrProfiles,
  loadRootPaths: loadSonarrRootPaths,
  loadLanguages: loadSonarrLanguages,
  loadTags: loadSonarrTags,
};

export default connect(null, mapPropsToAction)(Sonarr);