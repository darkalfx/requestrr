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
import { connect } from 'react-redux';
import { Alert } from "reactstrap";
import { getSettings } from "../store/actions/TvShowsClientsActions"
import { saveDisabledClient } from "../store/actions/TvShowsClientsActions"
import { saveSonarrClient } from "../store/actions/SonarrClientActions"
import { saveOmbiClient } from "../store/actions/TvShowsClientsActions"
import { saveOverseerrTvShowClient } from "../store/actions/OverseerrClientSonarrActions"
import Dropdown from "../components/Inputs/Dropdown"
import Sonarr from "../components/DownloadClients/Sonarr/Sonarr"
import Ombi from "../components/DownloadClients/Ombi"
import Overseerr from "../components/DownloadClients/Overseerr/TvShows/OverseerrTvShow"

// reactstrap components
import {
  Button,
  Card,
  CardHeader,
  CardBody,
  FormGroup,
  Form,
  Container,
  Row,
  Col
} from "reactstrap";
// core components
import UserHeader from "../components/Headers/UserHeader.jsx";


function TvShows(props) {
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [saveAttempted, setSaveAttempted] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [saveError, setSaveError] = useState("");
  const [client, setClient] = useState("");
  const [restrictions, setRestrictions] = useState("None");
  const [sonarr, setSonarr] = useState({});
  const [isSonarrValid, setIsSonarrValid] = useState(false);
  const [ombi, setOmbi] = useState({});
  const [isOmbiValid, setIsOmbiValid] = useState(false);
  const [overseerr, setOverseerr] = useState({});
  const [isOverseerrValid, setIsOverseerrValid] = useState(false);


  
  
  useEffect(() => {
    props.getSettings()
      .then((data) => {
        setIsLoading(false);
        setClient(data.payload.client);
        setSonarr(data.payload.sonarr);
        setOmbi(data.payload.ombi);
        setOverseerr(data.payload.overseerr);
        setRestrictions(data.payload.restrictions);
      });
  }, []);


  useEffect(() => {
    if (!isSaving) {
      if ((client === "Disabled"
        || (client === "Sonarr"
          && isSonarrValid)
        || (client === "Ombi"
          && isOmbiValid)
        || (client === "Overseerr"
          && isOverseerrValid)
      )) {
        setIsSaving(true);

        let saveAction = null;

        if (client === "Disabled") {
          saveAction = props.saveDisabledClient();
        }
        else if (client === "Ombi") {
          saveAction = props.saveOmbiClient({
            ombi: ombi,
            restrictions: restrictions
          });
        }
        else if (client === "Overseerr") {
          saveAction = props.saveOverseerrClient({
            overseerr: overseerr,
            restrictions: restrictions
          });
        }
        else if (client === "Sonarr") {
          saveAction = props.saveSonarrClient({
            sonarr: sonarr,
            restrictions: restrictions
          });
        }

        saveAction.then(data => {
          setIsSaving(false);
          setIsSubmitted(false);

          if (data.ok) {
            setSaveAttempted(true);
            setSaveError("");
            setSaveSuccess(true);
          }
          else {
            let error = "An unknown error occurred while saving.";

            if (typeof (data.error) === "string")
              error = data.error;

            setSaveAttempted(true);
            setSaveError(error);
            setSaveSuccess(false);
          }
        });
      }
      else {
        setSaveAttempted(true);
        setSaveError("Some fields are invalid, please fix them before saving.");
        setSaveSuccess(false);
        setIsSubmitted(false);
      }
    }
  }, [isSubmitted]);


  useEffect(() => {
    onClientChange()
  }, [client]);



  
  // const validateNonEmptyString = value => {
  //   return /\S/.test(value);
  // }


  const onClientChange = () => {
    setSonarr(props.settings.sonarr);
    setOmbi(props.settings.ombi);
    setOverseerr(props.settings.overseerr);
    setSaveAttempted(false);
    setIsSubmitted(false);
  };

  const onSaving = (e) => {
    e.preventDefault();
    setIsSubmitted(true);
  };





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
              <CardBody className={isLoading ? "fade" : "fade show"}>
                <Form className="complex">
                  <h6 className="heading-small text-muted mb-4">
                    General Settings
                  </h6>
                  <div className="pl-lg-4">
                    <Row>
                      <Col lg="6">
                        <Dropdown
                          name="Download Client"
                          value={client}
                          items={[{ name: "Disabled", value: "Disabled" }, { name: "Sonarr", value: "Sonarr" }, { name: "Overseerr", value: "Overseerr" }, { name: "Ombi", value: "Ombi" }]}
                          onChange={newClient => { setClient(newClient) }} />
                      </Col>
                      {
                        client !== "Disabled"
                          ? <>
                            <Col lg="6">
                              <Dropdown
                                name="Season Restrictions"
                                value={restrictions}
                                items={[{ name: "No restrictions", value: "None" }, { name: "Force all seasons", value: "AllSeasons" }, { name: "Force single season", value: "SingleSeason" }]}
                                onChange={newRestrictions => { setRestrictions(newRestrictions) }} />
                            </Col>
                          </>
                          : null
                      }
                    </Row>
                    <Row>
                      <Col lg="6">
                        {
                          props.settings.client !== client && props.settings.client !== "Disabled" ?
                            <Alert className="text-center" color="warning">
                              <strong>Changing the download client will delete all pending tv notifications.</strong>
                            </Alert>
                            : null
                        }
                      </Col>
                    </Row>
                  </div>
                  {
                    client !== "Disabled"
                      ? <>
                        <hr className="my-4" />
                        {
                          client === "Ombi"
                            ? <>
                              <Ombi settings={ombi} onChange={newOmbi => { setOmbi(newOmbi) }} onValidate={isOmbiValid => setIsOmbiValid(isOmbiValid)} isSubmitted={isSubmitted} />
                            </>
                            : null
                        }
                        {
                          client === "Overseerr"
                            ? <>
                              <Overseerr onChange={newOverseerr => setOverseerr(newOverseerr)} onValidate={isOverseerrValid => setIsOverseerrValid(isOverseerrValid)} isSubmitted={isSubmitted} isSaving={isSaving} />
                            </>
                            : null
                        }
                        {
                          client === "Sonarr"
                            ? <>
                              <Sonarr onChange={newSonarr => setSonarr(newSonarr)} onValidate={isSonarrValid => setIsSonarrValid(isSonarrValid)} isSubmitted={isSubmitted} isSaving={isSaving} />
                            </>
                            : null
                        }
                      </>
                      : null
                  }
                  <div className="pl-lg-4">
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
    settings: state.tvShows
  }
};


const mapPropsToAction = {
  getSettings: getSettings,
  saveDisabledClient: saveDisabledClient,
  saveSonarrClient: saveSonarrClient,
  saveOmbiClient: saveOmbiClient,
  saveOverseerrClient: saveOverseerrTvShowClient,
};

export default connect(mapPropsToState, mapPropsToAction)(TvShows);