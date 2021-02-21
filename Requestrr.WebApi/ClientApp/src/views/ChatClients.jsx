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
import React from "react";
import Loader from 'react-loader-spinner'
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

class ChatClients extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isLoading: true,
      isSaving: false,
      saveAttempted: false,
      saveSuccess: false,
      saveError: "",
      isCopyingLink: false,
      isTestingSettings: false,
      testSettingsRequested: false,
      testSettingsSuccess: false,
      testSettingsError: "",
      commandPrefix: "",
      monitoredChannels: [],
      statusMessage: "",
      enableRequestsThroughDirectMessages: true,
      chatClient: "",
      chatClientChanged: false,
      chatClientInvalid: false,
      clientId: "",
      clientIdChanged: false,
      clientIdInvalid: false,
      botToken: "",
      botTokenChanged: false,
      botTokenInvalid: false,
      tvShowRoles: [],
      movieRoles: [],
      automaticallyNotifyRequesters: true,
      notificationMode: "PrivateMessages",
      notificationChannels: [],
      automaticallyPurgeCommandMessages: true,
      displayHelpCommandInDMs: true,
    };

    this.onSaving = this.onSaving.bind(this);
    this.onTestSettings = this.onTestSettings.bind(this);
    this.onGenerateInviteLink = this.onGenerateInviteLink.bind(this);
    this.triggerBotTokenValidation = this.triggerBotTokenValidation.bind(this);
    this.triggerChatClientValidation = this.triggerChatClientValidation.bind(this);
    this.triggerClientIdValidation = this.triggerClientIdValidation.bind(this);
    this.validateClientId = this.validateClientId.bind(this);
    this.validateBotToken = this.validateBotToken.bind(this);
    this.validateClient = this.validateChatClient.bind(this);
  }

  componentDidMount() {
    this.props.getSettings(this.props.token)
      .then(data => {
        this.setState({
          isLoading: false,
          chatClient: this.props.settings.chatClient,
          clientId: this.props.settings.clientId,
          botToken: this.props.settings.botToken,
          statusMessage: this.props.settings.statusMessage,
          commandPrefix: this.props.settings.commandPrefix,
          monitoredChannels: this.props.settings.monitoredChannels,
          enableRequestsThroughDirectMessages: this.props.settings.enableRequestsThroughDirectMessages,
          tvShowRoles: this.props.settings.tvShowRoles,
          movieRoles: this.props.settings.movieRoles,
          automaticallyNotifyRequesters: this.props.settings.automaticallyNotifyRequesters,
          notificationMode: this.props.settings.notificationMode,
          notificationChannels: this.props.settings.notificationChannels,
          automaticallyPurgeCommandMessages: this.props.settings.automaticallyPurgeCommandMessages,
          displayHelpCommandInDMs: this.props.settings.displayHelpCommandInDMs,
        });
      });
  }

  onChatClientChange = event => {
    this.setState({
      chatClient: event.target.value,
      chatClientChanged: true
    }, () => this.triggerChatClientValidation());
  }

  triggerChatClientValidation() {
    this.setState({
      chatClientInvalid: !this.validateChatClient()
    });
  }

  validateChatClient() {
    return /\S/.test(this.state.chatClient);
  }

  onStatusMessageChange = event => {
    this.setState({ statusMessage: event.target.value });
  }

  onClientIdChange = event => {
    this.setState({
      clientId: event.target.value,
      clientIdChanged: true
    }, () => this.triggerClientIdValidation());
  }

  triggerClientIdValidation() {
    this.setState({
      clientIdInvalid: !this.validateClientId()
    });
  }

  validateClientId() {
    return /\S/.test(this.state.clientId);
  }

  onBotTokenChange = event => {
    this.setState({
      botToken: event.target.value,
      botTokenChanged: true
    }, () => this.triggerBotTokenValidation());
  }

  triggerBotTokenValidation() {
    this.setState({
      botTokenInvalid: !this.validateBotToken()
    });
  }

  validateBotToken() {
    return /\S/.test(this.state.botToken);
  }

  onCommandPrefixChange = event => {
    this.setState({ commandPrefix: event.target.value });
  }

  onSaving = e => {
    e.preventDefault();

    this.triggerChatClientValidation();
    this.triggerClientIdValidation();
    this.triggerBotTokenValidation();

    if (!this.isSaving) {
      if (this.validateChatClient()
        && this.validateBotToken()
        && this.validateClientId()) {
        this.setState({ isSaving: true });

        this.props.save({
          chatClient: this.state.chatClient,
          clientId: this.state.clientId,
          botToken: this.state.botToken,
          statusMessage: this.state.statusMessage,
          commandPrefix: this.state.commandPrefix,
          monitoredChannels: this.state.monitoredChannels,
          tvShowRoles: this.state.tvShowRoles,
          movieRoles: this.state.movieRoles,
          enableRequestsThroughDirectMessages: this.state.enableRequestsThroughDirectMessages,
          automaticallyNotifyRequesters: this.state.automaticallyNotifyRequesters,
          notificationMode: this.state.notificationMode,
          notificationChannels: this.state.notificationChannels,
          automaticallyPurgeCommandMessages: this.state.automaticallyPurgeCommandMessages,
          displayHelpCommandInDMs: this.state.displayHelpCommandInDMs,
        })
          .then(data => {
            this.setState({ isSaving: false });

            if (data.ok) {
              this.setState({
                savingAttempted: true,
                savingError: "",
                savingSuccess: true
              });
            }
            else {
              var error = "An unknown error occurred while saving.";

              if (typeof (data.error) === "string")
                error = data.error;

              this.setState({
                savingAttempted: true,
                savingError: error,
                savingSuccess: false
              });
            }
          });
      }
      else {
        this.setState({
          savingAttempted: true,
          savingError: "Some fields are invalid, please fix them before saving.",
          savingSuccess: false
        });
      }
    }
  }

  onTestSettings = e => {
    e.preventDefault();

    this.triggerChatClientValidation();
    this.triggerClientIdValidation();
    this.triggerBotTokenValidation();

    if (!this.state.isTestingSettings
      && this.validateChatClient()
      && this.validateBotToken()
      && this.validateClientId()) {
      this.setState({ isTestingSettings: true });

      this.props.testSettings({
        chatClient: this.state.chatClient,
        clientId: this.state.clientId,
        botToken: this.state.botToken,
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

  onGenerateInviteLink = e => {
    e.preventDefault();

    this.triggerChatClientValidation();
    this.triggerClientIdValidation();
    this.triggerBotTokenValidation();

    if (!this.state.isCopyingLink
      && this.validateChatClient()
      && this.validateBotToken()
      && this.validateClientId()) {
      this.setState({ isCopyingLink: true });

      var linkElement = document.getElementById("discordlink");
      linkElement.classList.remove("d-none");
      linkElement.focus();
      linkElement.select();
      document.execCommand('copy');
      linkElement.classList.add("d-none");

      let thisRef = this;
      setTimeout(function () { thisRef.setState({ isCopyingLink: false }) }, 3000);
    }
  }

  render() {
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
                <CardBody className={this.state.isLoading ? "fade" : "fade show"}>
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
                          <FormGroup className={this.state.clientIdInvalid ? "has-danger" : this.state.clientIdChanged ? "has-success" : ""}>
                            <label
                              className="form-control-label"
                              htmlFor="input-client-id"
                            >
                              Client Id
                            </label>
                            <Input
                              value={this.state.clientId} onChange={this.onClientIdChange}
                              className="form-control-alternative"
                              id="input-client-id"
                              placeholder="Enter client id"
                              type="text"
                            />
                            {
                              this.state.clientIdInvalid ? (
                                <Alert className="mt-3" color="warning">
                                  <strong>Client id is required.</strong>
                                </Alert>)
                                : null
                            }
                          </FormGroup>
                        </Col>
                        <Col lg="6">
                          <FormGroup className={this.state.botTokenInvalid ? "has-danger" : this.state.botTokenChanged ? "has-success" : ""}>
                            <label
                              className="form-control-label"
                              htmlFor="input-bot-token"
                            >
                              Bot Token
                            </label>
                            <Input
                              value={this.state.botToken} onChange={this.onBotTokenChange}
                              className="form-control-alternative"
                              id="input-bot-token"
                              placeholder="Enter bot token"
                              type="text"
                            />
                            {
                              this.state.botTokenInvalid ? (
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
                              placeholder="Enter roles here. Leave blank for all roles."
                              labelField="name"
                              valueField="id"
                              dropdownHandle={false}
                              selectedItems={this.state.tvShowRoles.map(x => { return { name: x, id: x } })}
                              items={this.state.tvShowRoles.map(x => { return { name: x, id: x } })}
                              onChange={newTvShowRoles => this.setState({ tvShowRoles: newTvShowRoles.filter(x => /\S/.test(x.id)).map(x => x.id.trim()) })} />
                          </FormGroup>
                        </Col>
                        <Col lg="6">
                          <FormGroup>
                            <MultiDropdown
                              name="Roles allowed to request movies"
                              create={true}
                              searchable={true}
                              placeholder="Enter roles here. Leave blank for all roles."
                              labelField="name"
                              valueField="id"
                              dropdownHandle={false}
                              selectedItems={this.state.movieRoles.map(x => { return { name: x, id: x } })}
                              items={this.state.movieRoles.map(x => { return { name: x, id: x } })}
                              onChange={newMovieRoles => this.setState({ movieRoles: newMovieRoles.filter(x => /\S/.test(x.id)).map(x => x.id.trim()) })} />
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
                              placeholder="Enter channels here. Leave blank for all channels."
                              labelField="name"
                              valueField="id"
                              dropdownHandle={false}
                              selectedItems={this.state.monitoredChannels.map(x => { return { name: x, id: x } })}
                              items={this.state.monitoredChannels.map(x => { return { name: x, id: x } })}
                              onChange={newMonitoredChannels => this.setState({ monitoredChannels: newMonitoredChannels.filter(x => /\S/.test(x.id)).map(x => x.id.trim().replace(/#/g, '').replace(/\s+/g, '-')) })} />
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
                              value={this.state.statusMessage} onChange={this.onStatusMessageChange}
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
                            value={this.state.notificationMode}
                            items={[{ name: "Disabled", value: "Disabled" }, { name: "Via private messages", value: "PrivateMessages" }, { name: "Via channel(s)", value: "Channels" }]}
                            onChange={newNotificationMode => this.setState({ notificationMode: newNotificationMode }, this.onValueChange)} />
                        </Col>
                        <Col lg="6">
                          {
                            this.state.notificationMode === "Channels"
                              ? <>
                                <FormGroup>
                                  <MultiDropdown
                                    name="Channel(s) to send notifications to"
                                    create={true}
                                    searchable={true}
                                    placeholder="Enter channels names here"
                                    labelField="name"
                                    valueField="id"
                                    dropdownHandle={false}
                                    selectedItems={this.state.notificationChannels.map(x => { return { name: x, id: x } })}
                                    items={this.state.notificationChannels.map(x => { return { name: x, id: x } })}
                                    onChange={newNotificationChannels => this.setState({ notificationChannels: newNotificationChannels.filter(x => /\S/.test(x.id)).map(x => x.id.trim().replace(/#/g, '').replace(/\s+/g, '-')) })} />
                                </FormGroup>
                              </>
                              : null
                          }
                        </Col>
                      </Row>
                      {
                        this.state.notificationMode !== "Disabled"
                          ? <>
                          <Row>
                            <Col lg="6">
                              {
                                <Alert className="text-center" color="warning">
                                    <strong>In order for notifications to work, make sure to enable the [Presence Intent] and [Server Members Intent] settings for the bot in the Discord Developper Portal</strong>
                                </Alert>
                              }
                            </Col>
                          </Row>
                            <Row>
                              <Col md="12">
                                <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                                  <Input
                                    className="custom-control-input"
                                    id="automaticRequests"
                                    type="checkbox"
                                    onChange={e => { this.setState({ automaticallyNotifyRequesters: !this.state.automaticallyNotifyRequesters }); }}
                                    checked={this.state.automaticallyNotifyRequesters}
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
                              onChange={e => { this.setState({ enableRequestsThroughDirectMessages: !this.state.enableRequestsThroughDirectMessages }); }}
                              checked={this.state.enableRequestsThroughDirectMessages}
                            />
                            <label
                              className="custom-control-label"
                              htmlFor="enableRequestDM"
                            >
                              <span className="text-muted">Enable anyone to request via a private message. <strong>(Those will ignore configured roles)</strong></span>
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
                              onChange={e => { this.setState({ automaticallyPurgeCommandMessages: !this.state.automaticallyPurgeCommandMessages }); }}
                              checked={this.state.automaticallyPurgeCommandMessages}
                            />
                            <label
                              className="custom-control-label"
                              htmlFor="deleteRequestMessages"
                            >
                              <span className="text-muted">Automatically delete user/bot messages (ex: !movie the matrix)</span>
                            </label>
                          </FormGroup>
                        </Col>
                      </Row>
                      <Row>
                        <Col md="12">
                          <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                            <Input
                              className="custom-control-input"
                              id="helpInDMs"
                              type="checkbox"
                              onChange={e => { this.setState({ displayHelpCommandInDMs: !this.state.displayHelpCommandInDMs }); }}
                              checked={this.state.displayHelpCommandInDMs}
                            />
                            <label
                              className="custom-control-label"
                              htmlFor="helpInDMs"
                            >
                              <span className="text-muted">Send the help command response in a private message instead of the current channel</span>
                            </label>
                          </FormGroup>
                        </Col>
                      </Row>
                    </div>
                    <div className="pl-lg-4">
                      <div className="mt-4">
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
                      </div>
                      <Row>
                        <Col>
                          <FormGroup className="text-right">
                            <Input id="discordlink" readOnly={true} className="d-none" value={"https://discord.com/api/oauth2/authorize?&client_id=" + this.state.clientId + "&scope=bot&permissions=522304"} />
                            <button onClick={this.onTestSettings} disabled={!(this.validateBotToken() && this.validateClientId())} className="btn mt-3 btn-icon btn-3 btn-default" type="button">
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
                            <button onClick={this.onGenerateInviteLink} disabled={!(this.validateBotToken() && this.validateClientId())} className="btn mt-3 btn-icon btn-3 btn-info" type="button">
                              <span className="btn-inner--icon"><i className="fas fa-copy"></i></span>
                              <span className="btn-inner--text">{this.state.isCopyingLink ? "Copied to clipboard!" : "Copy Invite Link"}</span>
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
                        <Col lg="4">
                          <FormGroup>
                            <label
                              className="form-control-label"
                              htmlFor="input-command-prefix"
                            >
                              Command Prefix
                            </label>
                            <Input
                              value={this.state.commandPrefix} onChange={this.onCommandPrefixChange}
                              className="form-control-alternative"
                              id="input-command-prefix"
                              placeholder="Enter command prefix (Optional)"
                              type="text"
                            />
                          </FormGroup>
                        </Col>
                        <Col lg="4">
                        </Col>
                      </Row>
                      <Row>
                        <Col>
                          <FormGroup className="mt-4">
                            {
                              this.state.savingAttempted && !this.state.isSaving ?
                                !this.state.savingSuccess ? (
                                  <Alert className="text-center" color="danger">
                                    <strong>{this.state.savingError}</strong>
                                  </Alert>)
                                  : <Alert className="text-center" color="success">
                                    <strong>Settings updated successfully.</strong>
                                  </Alert>
                                : null
                            }
                          </FormGroup>
                          <FormGroup className="text-right">
                            <button className="btn btn-icon btn-3 btn-primary" onClick={this.onSaving} disabled={this.state.isSaving} type="button">
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
