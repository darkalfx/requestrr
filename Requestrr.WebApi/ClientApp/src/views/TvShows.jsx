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
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { getSettings } from "../store/actions/TvShowsClientsActions"
import { saveDisabledClient } from "../store/actions/TvShowsClientsActions"
import { saveSonarrClient } from "../store/actions/TvShowsClientsActions"
import { saveOmbiClient } from "../store/actions/TvShowsClientsActions"
import ValidatedTextbox from "../components/Inputs/ValidatedTextbox"
import Dropdown from "../components/Inputs/Dropdown"
import Sonarr from "../components/DownloadClients/Sonarr"
import Ombi from "../components/DownloadClients/Ombi"

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

class TvShows extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      isSubmitted: false,
      isLoading: true,
      isSaving: false,
      saveAttempted: false,
      saveSuccess: false,
      saveError: "",
      client: "",
      restrictions: "None",
      command: "",
      sonarr: {},
      isSonarrValid: false,
      ombi: {},
      isOmbiValid: false,
      isCommandValid: false,
    };

    this.onSaving = this.onSaving.bind(this);
    this.onClientChange = this.onClientChange.bind(this);
    this.validateNonEmptyString = this.validateNonEmptyString.bind(this);
  }

  componentDidMount() {
    this.props.getSettings()
      .then(data => {
        this.setState({
          isLoading: false,
          client: this.props.settings.client,
          sonarr: this.props.settings.sonarr,
          ombi: this.props.settings.ombi,
          command: this.props.settings.command,
          restrictions: this.props.settings.restrictions,
        });
      });
  }

  validateNonEmptyString = value => {
    return /\S/.test(value);
  }

  onClientChange() {
    this.setState({
      sonarr: this.props.settings.sonarr,
      ombi: this.props.settings.ombi,
      savingAttempted: false,
      isSubmitted: false,
    });
  }

  onSaving = e => {
    e.preventDefault();
    this.setState({ isSubmitted: true });

    if (!this.state.isSaving) {
      if ((this.state.client === "Disabled"
        || (this.state.client === "Sonarr"
          && this.state.isSonarrValid
          && this.state.isCommandValid)
        || (this.state.client === "Ombi"
          && this.state.isOmbiValid
          && this.state.isCommandValid)
      )) {
        this.setState({ isSaving: true });

        let saveAction = null;

        if (this.state.client === "Disabled") {
          saveAction = this.props.saveDisabledClient();
        }
        else if (this.state.client === "Ombi") {
          saveAction = this.props.saveOmbiClient({
            ombi: this.state.ombi,
            command: this.state.command,
            restrictions: this.state.restrictions
          });
        }
        else if (this.state.client === "Sonarr") {
          saveAction = this.props.saveSonarrClient({
            sonarr: this.state.sonarr,
            command: this.state.command,
            restrictions: this.state.restrictions
          });
        }

        saveAction.then(data => {
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

  render() {
    return (
      <>
        <UserHeader title="TV Shows" description="This page is for configuring the connection between your bot and your favorite tv shows download client" />
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
                          <Dropdown
                            name="Download Client"
                            value={this.state.client}
                            items={[{ name: "Disabled", value: "Disabled" }, { name: "Sonarr", value: "Sonarr" }, { name: "Ombi", value: "Ombi" }]}
                            onChange={newClient => { this.setState({ client: newClient }, this.onClientChange) }} />
                        </Col>
                      </Row>
                    </div>
                    {
                      this.state.client !== "Disabled"
                        ? <>
                          <hr className="my-4" />
                          {
                            this.state.client === "Ombi"
                              ? <>
                                <Ombi settings={this.state.ombi} onChange={newOmbi => this.setState({ ombi: newOmbi })} onValidate={isOmbiValid => this.setState({ isOmbiValid: isOmbiValid })} isSubmitted={this.state.isSubmitted} />
                              </>
                              : null
                          }
                          {
                            this.state.client === "Sonarr"
                              ? <>
                                <Sonarr settings={this.state.sonarr} onChange={newSonarr => this.setState({ sonarr: newSonarr })} onValidate={isSonarrValid => this.setState({ isSonarrValid: isSonarrValid })} isSubmitted={this.state.isSubmitted} />
                              </>
                              : null
                          }
                          <hr className="my-4" />
                          <h6 className="heading-small text-muted mb-4">
                            Chat Command Options
                        </h6>
                        </>
                        : null
                    }
                    <div className="pl-lg-4">
                      {
                        this.state.client !== "Disabled"
                          ? <>
                            <Row>
                              <Col lg="4">
                                <ValidatedTextbox
                                  name="Command"
                                  placeholder="Enter chat command"
                                  alertClassName="mt-3"
                                  errorMessage="A chat command is required."
                                  isSubmitted={this.state.isSubmitted}
                                  value={this.state.command}
                                  validation={this.validateNonEmptyString}
                                  onChange={newCommand => this.setState({ command: newCommand })}
                                  onValidate={isValid => this.setState({ isCommandValid: isValid })} />
                              </Col>
                              <Col lg="4">
                                <Dropdown
                                  name="Restrictions"
                                  value={this.state.restrictions}
                                  items={[{ name: "No restrictions", value: "None" }, { name: "Force all seasons", value: "AllSeasons" }, { name: "Force single season", value: "SingleSeason" }]}
                                  onChange={newRestrictions => { this.setState({ restrictions: newRestrictions }) }} />
                              </Col>
                            </Row>
                          </>
                          : null
                      }
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
    settings: state.tvShows
  }
};

const mapPropsToAction = {
  getSettings: getSettings,
  saveDisabledClient: saveDisabledClient,
  saveSonarrClient: saveSonarrClient,
  saveOmbiClient: saveOmbiClient,
};

export default connect(mapPropsToState, mapPropsToAction)(TvShows);