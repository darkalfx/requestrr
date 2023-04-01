/*!

=========================================================
* Argon Dashboard React - v1.0.0
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-react
* Copyright 2019 Creative Tim (https://www.creative-tim.com)
* Licensed under MIT (https://github.com/creativetimofficial/argon-dashboard-react/blob/master/LICENSE.md)

* Coded by Creative Tim

=========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

*/
import { useEffect, useState } from "react";
import { Oval } from 'react-loader-spinner'
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { testSettings } from "../store/actions/ChatClientsActions"
import { getSettings } from "../store/actions/ChatClientsActions"
import { save } from "../store/actions/ChatClientsActions"
import MultiDropdown from "../components/Inputs/MultiDropdown"
import Dropdown from "../components/Inputs/Dropdown"

// reactstrap components
import {
  Button,
  Card,
  CardHeader,
  CardBody,
  FormGroup,
  Form,
  Input,
  Container,
  Row,
  Col
} from "reactstrap";
// core components
import UserHeader from "../components/Headers/UserHeader.jsx";


function ChatClients(props) {
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [saveAttempted, setSaveAttempted] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [saveError, setSaveError] = useState("");
  const [isCopyingLink, setIsCopyingLink] = useState(false);
  const [isTestingSettings, setIsTestingSettings] = useState(false);
  const [testSettingsRequested, setTestSettingsRequested] = useState(false);
  const [testSettingsSuccess, setTestSettingsSuccess] = useState(false);
  const [testSettingsError, setTestSettingsError] = useState("");
  const [monitoredChannels, setMonitoredChannels] = useState([]);
  const [statusMessage, setStatusMessage] = useState("");
  const [enableRequestsThroughDirectMessages, setEnableRequestsThroughDirectMessages] = useState(true);
  const [chatClient, setChatClient] = useState("");
  const [chatClientChanged, setChatClientChanged] = useState(false);
  const [chatClientInvalid, setChatClientInvalid] = useState(false);
  const [clientId, setClientId] = useState("");
  const [clientIdChanged, setClientIdChanged] = useState(false);
  const [clientIdInvalid, setClientIdInvalid] = useState(false);
  const [botToken, setBotToken] = useState("");
  const [botTokenChanged, setBotTokenChanged] = useState(false);
  const [botTokenInvalid, setBotTokenInvalid] = useState(false);
  const [tvShowRoles, setTvShowRoles] = useState([]);
  const [movieRoles, setMovieRoles] = useState([]);
  const [automaticallyNotifyRequesters, setAutomaticallyNotifyRequesters] = useState(true);
  const [notificationMode, setNotificationMode] = useState("PrivateMessages");
  const [notificationChannels, setNotificationChannels] = useState([]);
  const [automaticallyPurgeCommandMessages, setAutomaticallyPurgeCommandMessages] = useState(true);
  const [language, setLanguage] = useState("english");
  const [availableLanguages, setAvailableLanguages] = useState([]);



  useEffect(() => {
    props.getSettings(props.token)
      .then(data => {
        setIsLoading(false);
        setChatClient(data.payload.client);
        setClientId(data.payload.clientId);
        setBotToken(data.payload.botToken);
        setStatusMessage(data.payload.statusMessage);
        setMonitoredChannels(data.payload.monitoredChannels);
        setEnableRequestsThroughDirectMessages(data.payload.enableRequestsThroughDirectMessages);
        setTvShowRoles(data.payload.tvShowRoles);
        setMovieRoles(data.payload.movieRoles);
        setAutomaticallyNotifyRequesters(data.payload.automaticallyNotifyRequesters);
        setNotificationMode(data.payload.notificationMode);
        setNotificationChannels(data.payload.notificationChannels);
        setAutomaticallyPurgeCommandMessages(data.payload.automaticallyPurgeCommandMessages);
        setLanguage(data.payload.language);
        setAvailableLanguages(data.payload.availableLanguages);
      });
  }, []);


  useEffect(() => {
    if (chatClientChanged)
      triggerChatClientValidation();
  }, [chatClientChanged]);


  useEffect(() => {
    if (clientIdChanged)
      triggerClientIdValidation()
  }, [clientIdChanged]);


  useEffect(() => {
    if (botTokenChanged)
      triggerBotTokenValidation()
  }, [botTokenChanged]);



  
  const validateChatClient = () => {
    return /\S/.test(chatClient);
  };

  const validateClientId = () => {
    return /\S/.test(clientId);
  };

  const validateBotToken = () => {
    return /\S/.test(botToken);
  };



  // const onChatClientChange = (event) => {
  //   setChatClient(event.target.value);
  //   setChatClientChanged(true);
  // };

  const triggerChatClientValidation = () => {
    setChatClientInvalid(!validateChatClient());
  };

  const onStatusMessageChange = (event) => {
    setStatusMessage(event.target.value);
  };

  const onClientIdChange = (event) => {
    setClientId(event.target.value);
    setClientIdChanged(true);
  };


  const triggerClientIdValidation = () => {
    setClientIdInvalid(!validateClientId());
  };


  const onBotTokenChange = (event) => {
    setBotToken(event.target.value);
    setBotTokenChanged(true);
  };

  const triggerBotTokenValidation = () => {
    setBotTokenInvalid(!validateBotToken());
  };


  
  const onSaving = (e) => {
    e.preventDefault();

    triggerChatClientValidation();
    triggerClientIdValidation();
    triggerBotTokenValidation();

    if (!isSaving) {
      if (validateChatClient()
        && validateBotToken()
        && validateClientId()) {
        setIsSaving(true);

        props.save({
          client: chatClient,
          clientId: clientId,
          botToken: botToken,
          statusMessage: statusMessage,
          monitoredChannels: monitoredChannels,
          tvShowRoles: tvShowRoles,
          movieRoles: movieRoles,
          enableRequestsThroughDirectMessages: enableRequestsThroughDirectMessages,
          automaticallyNotifyRequesters: automaticallyNotifyRequesters,
          notificationMode: notificationMode,
          notificationChannels: notificationChannels,
          automaticallyPurgeCommandMessages: automaticallyPurgeCommandMessages,
          language: language,
        })
          .then(data => {
            setIsSaving(false);

            if (data.ok) {
              setSaveAttempted(true);
              setSaveError("");
              setSaveSuccess(true);
            } else {
              let error = "An unknown error occurred while saving.";

              if (typeof (data.error) === "string")
                error = data.error;

              setSaveAttempted(true);
              setSaveError(error);
              setSaveSuccess(false);
            }
          });
      } else {
        setSaveAttempted(true);
        setSaveError("Some fields are invalid, please fix them before saving.");
        setSaveSuccess(false);
      }
    }
  }

  const onTestSettings = (e) => {
    e.preventDefault();

    triggerChatClientValidation();
    triggerClientIdValidation();
    triggerBotTokenValidation();

    if (!isTestingSettings
      && validateChatClient()
      && validateBotToken()
      && validateClientId()) {
      setIsTestingSettings(true);

      props.testSettings({
        chatClient: chatClient,
        clientId: clientId,
        botToken: botToken,
      })
        .then(data => {
          setIsTestingSettings(false);

          if (data.ok) {
            setTestSettingsRequested(true);
            setTestSettingsError("");
            setTestSettingsSuccess(true);
          } else {
            let error = "An unknown error occurred while testing the settings";

            if (typeof (data.error) === "string")
              error = data.error;

            setTestSettingsRequested(true);
            setTestSettingsError(error);
            setTestSettingsSuccess(false);
          }
        });
    }
  };

  const onGenerateInviteLink = (e) => {
    e.preventDefault();

    triggerChatClientValidation();
    triggerClientIdValidation();
    triggerBotTokenValidation();

    if (!isCopyingLink
      && validateChatClient()
      && validateBotToken()
      && validateClientId()) {
      setIsCopyingLink(true);

      let linkElement = document.getElementById("discordlink");
      linkElement.classList.remove("d-none");
      linkElement.focus();
      linkElement.select();
      let text = linkElement.value;
      navigator.clipboard.writeText(text);
      linkElement.classList.add("d-none");

      // let thisRef = this;
      setTimeout(() => { setIsCopyingLink(false) }, 3000);
    }
  };





  return (
    <>
      <UserHeader title="Chat Client" description="This page is for configuring your bot to your favorite chatting application" />
      <Container className="mt--7" fluid>
        <Row>
          <Col className="order-xl-1" xl="12">
            <Card className="bg-secondary shadow">
              <CardHeader className="bg-white border-0">
                <Row className="align-items-center">
                  <Col xs="8">
                    <h3 className="mb-0">Configuration</h3>
                  </Col>
                  <Col className="text-right" xs="4">
                    <Button
                      color="primary"
                      href="#pablo"
                      onClick={e => e.preventDefault()}
                      size="sm"
                    >
                      Settings
                    </Button>
                  </Col>
                </Row>
              </CardHeader>
              <CardBody className={isLoading ? "fade" : "fade show"}>
                <Form className="complex">
                  <h6 className="heading-small text-muted mb-4">
                    General Settings
                  </h6>
                  <div className="pl-lg-4">
                    <Row>
                      <Col lg="6">
                        <FormGroup>
                          <label
                            className="form-control-label"
                            htmlFor="input-client"
                          >
                            Chat Client
                          </label>
                          <Input id="input-client" value="Discord" disabled="disabled" className="form-control" type="text" />
                        </FormGroup>
                      </Col>
                    </Row>
                  </div>
                  <hr className="my-4" />
                  <div>
                    <h6 className="heading-small text-muted mb-4">
                      Discord Bot Settings
                    </h6>
                  </div>
                  <div className="pl-lg-4">
                    <Row>
                      <Col lg="6">
                        <FormGroup className={clientIdInvalid ? "has-danger" : clientIdChanged ? "has-success" : ""}>
                          <label
                            className="form-control-label"
                            htmlFor="input-client-id"
                          >
                            Application Id
                          </label>
                          <Input
                            value={clientId} onChange={onClientIdChange}
                            className="form-control-alternative"
                            id="input-client-id"
                            placeholder="Enter client id"
                            type="text"
                          />
                          {
                            clientIdInvalid ? (
                              <Alert className="mt-3" color="warning">
                                <strong>Client id is required.</strong>
                              </Alert>)
                              : null
                          }
                        </FormGroup>
                      </Col>
                      <Col lg="6">
                        <FormGroup className={botTokenInvalid ? "has-danger" : botTokenChanged ? "has-success" : ""}>
                          <label
                            className="form-control-label"
                            htmlFor="input-bot-token"
                          >
                            Bot Token
                          </label>
                          <Input
                            value={botToken} onChange={onBotTokenChange}
                            className="form-control-alternative"
                            id="input-bot-token"
                            placeholder="Enter bot token"
                            type="text"
                          />
                          {
                            botTokenInvalid ? (
                              <Alert className="mt-3" color="warning">
                                <strong>Bot token is required.</strong>
                              </Alert>)
                              : null
                          }
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col lg="6">
                        <FormGroup>
                          <MultiDropdown
                            name="Roles allowed to request tv shows"
                            create={true}
                            searchable={true}
                            placeholder="Enter role ids here. Leave blank for all roles."
                            labelField="name"
                            valueField="id"
                            dropdownHandle={false}
                            selectedItems={tvShowRoles.map(x => { return { name: x, id: x } })}
                            items={tvShowRoles.map(x => { return { name: x, id: x } })}
                            onChange={newTvShowRoles => setTvShowRoles(newTvShowRoles.filter(x => /\S/.test(x.id)).map(x => x.id.trim()))} />
                        </FormGroup>
                      </Col>
                      <Col lg="6">
                        <FormGroup>
                          <MultiDropdown
                            name="Roles allowed to request movies"
                            create={true}
                            searchable={true}
                            placeholder="Enter role ids here. Leave blank for all roles."
                            labelField="name"
                            valueField="id"
                            dropdownHandle={false}
                            selectedItems={movieRoles.map(x => { return { name: x, id: x } })}
                            items={movieRoles.map(x => { return { name: x, id: x } })}
                            onChange={newMovieRoles => setMovieRoles(newMovieRoles.filter(x => /\S/.test(x.id)).map(x => x.id.trim()))} />
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col md="12">
                        <FormGroup>
                          <MultiDropdown
                            name="Channel(s) to monitor"
                            create={true}
                            searchable={true}
                            placeholder="Enter channels ids here. Leave blank for all channels."
                            labelField="name"
                            valueField="id"
                            dropdownHandle={false}
                            selectedItems={monitoredChannels.map(x => { return { name: x, id: x } })}
                            items={monitoredChannels.map(x => { return { name: x, id: x } })}
                            onChange={newMonitoredChannels => setMonitoredChannels(newMonitoredChannels.filter(x => /\S/.test(x.id)).map(x => x.id.trim().replace(/#/g, '').replace(/\s+/g, '-')))} />
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col lg="6">
                        <FormGroup>
                          <label
                            className="form-control-label"
                            htmlFor="input-status-message"
                          >
                            Status Message
                          </label>
                          <Input
                            value={statusMessage} onChange={onStatusMessageChange}
                            className="form-control-alternative"
                            id="input-status-message"
                            placeholder="Enter status message (Optional)"
                            type="text"
                          />
                        </FormGroup>
                      </Col>
                    </Row>
                  </div>
                  <div>
                    <h6 className="heading-small text-muted mt-4">
                      Discord Notification Settings
                    </h6>
                  </div>
                  <div className="pl-lg-4">
                    <Row>
                      <Col lg="6">
                        <Dropdown
                          name="Notifications"
                          value={notificationMode}
                          items={[{ name: "Disabled", value: "Disabled" }, { name: "Via private messages", value: "PrivateMessages" }, { name: "Via channel(s)", value: "Channels" }]}
                          onChange={newNotificationMode => setNotificationMode(newNotificationMode)} />
                      </Col>
                      <Col lg="6">
                        {
                          notificationMode === "Channels"
                            ? <>
                              <FormGroup>
                                <MultiDropdown
                                  name="Channel(s) to send notifications to"
                                  create={true}
                                  searchable={true}
                                  placeholder="Enter channels ids here"
                                  labelField="name"
                                  valueField="id"
                                  dropdownHandle={false}
                                  selectedItems={notificationChannels.map(x => { return { name: x, id: x } })}
                                  items={notificationChannels.map(x => { return { name: x, id: x } })}
                                  onChange={newNotificationChannels => setNotificationChannels(newNotificationChannels.filter(x => /\S/.test(x.id)).map(x => x.id.trim().replace(/#/g, '').replace(/\s+/g, '-'))) } />
                              </FormGroup>
                            </>
                            : null
                        }
                      </Col>
                    </Row>
                    {
                      notificationMode !== "Disabled"
                        ? <>
                          <Row>
                            <Col md="12">
                              <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                                <Input
                                  className="custom-control-input"
                                  id="automaticRequests"
                                  type="checkbox"
                                  onChange={e => { setAutomaticallyNotifyRequesters(!automaticallyNotifyRequesters); }}
                                  checked={automaticallyNotifyRequesters}
                                />
                                <label
                                  className="custom-control-label"
                                  htmlFor="automaticRequests"
                                >
                                  <span className="text-muted">Automatically notify users of downloaded content when they make requests</span>
                                </label>
                              </FormGroup>
                            </Col>
                          </Row>
                        </>
                        : null
                    }
                  </div>
                  <div>
                    <h6 className="heading-small text-muted mt-4">
                      Discord Miscellaneous Settings
                    </h6>
                  </div>
                  <div className="pl-lg-4">
                    <Row>
                      <Col md="12">
                        <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                          <Input
                            className="custom-control-input"
                            id="enableRequestDM"
                            type="checkbox"
                            onChange={e => { setEnableRequestsThroughDirectMessages(!enableRequestsThroughDirectMessages); }}
                            checked={enableRequestsThroughDirectMessages}
                          />
                          <label
                            className="custom-control-label"
                            htmlFor="enableRequestDM"
                          >
                            <span className="text-muted">Enable requesting via a private message, all role restrictions will be ignored, even on the servers.<br /><strong>(It might take up to an hour for the commands to show up on your servers.)</strong></span>
                          </label>
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col md="12">
                        <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                          <Input
                            className="custom-control-input"
                            id="deleteRequestMessages"
                            type="checkbox"
                            onChange={e => { setAutomaticallyPurgeCommandMessages(!automaticallyPurgeCommandMessages); }}
                            checked={automaticallyPurgeCommandMessages}
                          />
                          <label
                            className="custom-control-label"
                            htmlFor="deleteRequestMessages"
                          >
                            <span className="text-muted">Hide slash command request messages in channels.</span>
                          </label>
                        </FormGroup>
                      </Col>
                    </Row>
                    <Row>
                      <Col md="12">
                      </Col>
                    </Row>
                  </div>
                  <div className="pl-lg-4">
                    <div className="mt-4">
                      {
                        testSettingsRequested && !isTestingSettings ?
                          !testSettingsSuccess ? (
                            <Alert className="text-center" color="danger">
                              <strong>{testSettingsError}.</strong>
                            </Alert>)
                            : <Alert className="text-center" color="success">
                              <strong>The specified settings are valid.</strong>
                            </Alert>
                          : null
                      }
                    </div>
                    <Row>
                      <Col>
                        <FormGroup className="text-right">
                          <Input id="discordlink" readOnly={true} className="d-none" value={"https://discord.com/api/oauth2/authorize?client_id=" + clientId + "&permissions=522304&scope=bot%20applications.commands"} />
                          <button onClick={onTestSettings} disabled={!(validateBotToken() && validateClientId())} className="btn mt-3 btn-icon btn-3 btn-default" type="button">
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
                          <button onClick={onGenerateInviteLink} disabled={!(validateBotToken() && validateClientId())} className="btn mt-3 btn-icon btn-3 btn-info" type="button">
                            <span className="btn-inner--icon"><i className="fas fa-copy"></i></span>
                            <span className="btn-inner--text">{isCopyingLink ? "Copied to clipboard!" : "Copy Invite Link"}</span>
                          </button>
                        </FormGroup>
                      </Col>
                    </Row>
                  </div>
                  <hr className="my-4" />
                  <h6 className="heading-small text-muted mb-4">
                    Chat Command Options
                  </h6>
                  <div className="pl-lg-4">
                    <Row>
                      <Col lg="6">
                        <Dropdown
                          name="Language"
                          value={language}
                          items={availableLanguages.map(x => { return { name: x, value: x } })}
                          onChange={newLanguage => setLanguage(newLanguage)} />
                      </Col>
                    </Row>
                    <Row>
                      <Col>
                        <FormGroup className="mt-4">
                          {
                            saveAttempted && !isSaving ?
                              !saveSuccess ? (
                                <Alert className="text-center" color="danger">
                                  <strong>{saveError}</strong>
                                </Alert>)
                                : <Alert className="text-center" color="success">
                                  <strong>Settings updated successfully.</strong>
                                </Alert>
                              : null
                          }
                        </FormGroup>
                        <FormGroup className="text-right">
                          <button className="btn btn-icon btn-3 btn-primary" onClick={onSaving} disabled={isSaving} type="button">
                            <span className="btn-inner--icon"><i className="fas fa-save"></i></span>
                            <span className="btn-inner--text">Save Changes</span>
                          </button>
                        </FormGroup>
                      </Col>
                    </Row>
                  </div>
                </Form>
              </CardBody>
            </Card>
          </Col>
        </Row>
      </Container>
    </>
  );
}

const mapPropsToState = state => {
  return {
    settings: state.chatClients
  }
};

const mapPropsToAction = {
  testSettings: testSettings,
  getSettings: getSettings,
  save: save
};

export default connect(mapPropsToState, mapPropsToAction)(ChatClients);
