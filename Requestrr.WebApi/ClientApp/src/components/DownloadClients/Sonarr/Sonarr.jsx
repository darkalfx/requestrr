
import { useEffect, useRef, useState } from "react";
import { Oval } from 'react-loader-spinner';
import { useDispatch, useSelector } from 'react-redux';
import { Alert } from "reactstrap";
import { testSonarrSettings as testSettings } from "../../../store/actions/SonarrClientActions";
import { setSonarrConnectionSettings as setConnectionSettings } from "../../../store/actions/SonarrClientActions";
import ValidatedTextbox from "../../Inputs/ValidatedTextbox";
import Textbox from "../../Inputs/Textbox";
import Dropdown from "../../Inputs/Dropdown";
import SonarrCategoryList from "./SonarrCategoryList";

import {
  FormGroup,
  Input,
  Row,
  Col
} from "reactstrap";


function Sonarr(props) {
  const [isTestingSettings, setIsTestingSettings] = useState(false);
  const [testSettingsRequested, setTestSettingsRequested] = useState(false);
  const [testSettingsSuccess, setTestSettingsSuccess] = useState(false);
  const [testSettingsError, setTestSettingsError] = useState("");
  const [hostname, setHostname] = useState("");
  const [isHostnameValid, setIsHostnameValid] = useState(false);
  const [port, setPort] = useState("7878");
  const [isPortValid, setIsPortValid] = useState(false);
  const [apiKey, setApiKey] = useState("");
  const [isApiKeyValid, setIsApiKeyValid] = useState(false);
  const [useSSL, setUseSSL] = useState(false);
  const [apiVersion, setApiVersion] = useState("");
  const [baseUrl, setBaseUrl] = useState("");
  const [searchNewRequests, setSearchNewRequests] = useState(true);
  const [monitorNewRequests, setMonitorNewRequests] = useState(true);

  const pastState = useRef();

  const reduxState = useSelector((state) => {
    return {
      settings: state.tvShows.sonarr
    }
  });
  const dispatch = useDispatch();


  useEffect(() => {
    updateStateFromProps();
  }, []);


  useEffect(() => {
    const prevState = pastState.past;
    pastState.past = reduxState;

    let previousNames = prevState === undefined ? [] : prevState.settings.categories.map(x => x.name);
    let currentNames = reduxState.settings.categories.map(x => x.name);

    if (!(prevState?.settings?.profiles?.length === reduxState.settings.profiles.length && prevState?.settings?.profiles?.reduce((a, b, i) => a && reduxState.settings.profiles[i], true))
      || !(prevState?.settings?.paths?.length === reduxState.settings.paths.length && prevState?.settings?.paths?.reduce((a, b, i) => a && reduxState.settings.paths[i], true))
      || !(previousNames.length === currentNames.length && currentNames.every((value, index) => previousNames[index] === value))) {
      onValueChange();
    }
  });


  useEffect(() => {
    onValueChange();
  }, [apiVersion, apiKey, hostname, port, baseUrl, monitorNewRequests, searchNewRequests, useSSL, reduxState.settings.areLanguagesValid]);




  const validateNonEmptyString = (value) => {
    return /\S/.test(value);
  };

  const validatePort = (value) => {
    return /^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$/.test(value);
  };


  const validateCategoryName = (value) => {
    if (!/\S/.test(value)) {
      return false;
    } else if (/^[\w-]{1,32}$/.test(value)) {
      const names = reduxState.settings.categories.map((x) => x.name);

      if (new Set(names).size !== names.length)
        return false;
    } else {
      return false;
    }

    return true;
  };




  const updateStateFromProps = () => {
    setIsTestingSettings(false);
    setTestSettingsRequested(false);
    setTestSettingsSuccess(false);
    setTestSettingsError("");
    setHostname(reduxState.settings.hostname);
    setIsHostnameValid(validateNonEmptyString(reduxState.settings.hostname));
    setPort(reduxState.settings.port);
    setIsPortValid(validatePort(reduxState.settings.port));
    setApiKey(reduxState.settings.apiKey);
    setIsApiKeyValid(validateNonEmptyString(reduxState.settings.apiKey));
    setUseSSL(reduxState.settings.useSSL);
    setApiVersion(reduxState.settings.version);
    setBaseUrl(reduxState.settings.baseUrl);
    setSearchNewRequests(reduxState.settings.searchNewRequests);
    setMonitorNewRequests(reduxState.settings.monitorNewRequests);
  };


  const onUseSSLChanged = (event) => {
    setUseSSL(!useSSL);
    onValueChange();
  };


  const onTestSettings = (e) => {
    e.preventDefault();

    if (!isTestingSettings
      && isHostnameValid
      && isPortValid
      && isApiKeyValid) {
      setIsTestingSettings(true);

      dispatch(testSettings({
        hostname: hostname,
        baseUrl: baseUrl,
        port: port,
        apiKey: apiKey,
        useSSL: useSSL,
        version: apiVersion,
      }))
        .then(data => {
          if (data.ok) {
            setTestSettingsRequested(true);
            setTestSettingsError("");
            setTestSettingsSuccess(true);
          }
          else {
            var error = "An unknown error occurred while testing the settings";

            if (typeof (data.error) === "string")
              error = data.error;

            setTestSettingsRequested(true);
            setTestSettingsError(error);
            setTestSettingsSuccess(false);
          }

          setIsTestingSettings(false);
        });
    }
  }


  const onValueChange = () => {
    dispatch(setConnectionSettings({
      hostname: hostname,
      baseUrl: baseUrl,
      port: port,
      apiKey: apiKey,
      useSSL: useSSL,
      version: apiVersion,
    }));


    props.onChange({
      hostname: hostname,
      baseUrl: baseUrl,
      port: port,
      apiKey: apiKey,
      useSSL: useSSL,
      version: apiVersion,
      searchNewRequests: searchNewRequests,
      monitorNewRequests: monitorNewRequests,
    });

    onValidate();
  };


  const onValidate = () => {
    const isLanguageValid = apiVersion !== "2" ? reduxState.settings.areLanguagesValid : true;
    props.onValidate(
      isApiKeyValid &&
      isHostnameValid &&
      isPortValid &&
      reduxState.settings.areProfilesValid &&
      reduxState.settings.arePathsValid &&
      reduxState.settings.categories.every((x) => validateCategoryName(x.name)) &&
      isLanguageValid
    );
  }




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
              value={apiVersion}
              items={[{ name: "Version 2", value: "2" }, { name: "Version 3", value: "3" }]}
              onChange={newApiVersion => { setApiVersion(newApiVersion) }} />
          </Col>
          <Col lg="6">
            <ValidatedTextbox
              name="API Key"
              placeholder="Enter api key"
              alertClassName="mt-3 mb-0"
              errorMessage="api key is required."
              isSubmitted={props.isSubmitted}
              value={apiKey}
              validation={validateNonEmptyString}
              onChange={newApiKey => { setApiKey(newApiKey) }}
              onValidate={isValid => setIsApiKeyValid(isValid)} />
          </Col>
        </Row>
        <Row>
          <Col lg="6">
            <ValidatedTextbox
              name="Host or IP"
              placeholder="Enter host or ip"
              alertClassName="mt-3 mb-0"
              errorMessage="Hostname is required."
              isSubmitted={props.isSubmitted}
              value={hostname}
              validation={validateNonEmptyString}
              onChange={newHostname => { setHostname(newHostname) }}
              onValidate={isValid => setIsHostnameValid(isValid)} />
          </Col>
          <Col lg="6">
            <ValidatedTextbox
              name="Port"
              placeholder="Enter port"
              alertClassName="mt-3 mb-0"
              errorMessage="Please enter a valid port."
              isSubmitted={props.isSubmitted}
              value={port}
              validation={validatePort}
              onChange={newPort => { setPort(newPort) }}
              onValidate={isValid => setIsPortValid(isValid)} />
          </Col>
        </Row>
        <Row>
          <Col lg="6">
            <Textbox
              name="Base Url"
              placeholder="Enter base url configured in Sonarr, leave empty if none configured."
              value={baseUrl}
              onChange={newBaseUrl => { setBaseUrl(newBaseUrl) }} />
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
                onChange={onUseSSLChanged}
                checked={useSSL}
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
                testSettingsRequested && !isTestingSettings ?
                  !testSettingsSuccess ? (
                    <Alert className="text-center" color="danger">
                      <strong>{testSettingsError}</strong>
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
              <button onClick={onTestSettings} disabled={!isHostnameValid || !isPortValid || !isApiKeyValid} className="btn btn-icon btn-3 btn-default" type="button">
                <span className="btn-inner--icon">
                  {
                    isTestingSettings ? (
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
      <SonarrCategoryList isSubmitted={props.isSubmitted} isSaving={props.isSaving} apiVersion={apiVersion} canConnect={isHostnameValid && isPortValid && isApiKeyValid} />
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
                onChange={e => setMonitorNewRequests(!monitorNewRequests)}
                checked={monitorNewRequests}
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
                onChange={e => setSearchNewRequests(!searchNewRequests)}
                checked={searchNewRequests}
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

export default Sonarr;