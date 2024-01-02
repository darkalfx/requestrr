
import { useEffect, useRef, useState } from "react";
import { Oval } from 'react-loader-spinner'
import { useDispatch, useSelector } from 'react-redux';
import { Alert } from "reactstrap";
import { loadSonarrProfiles as loadProfiles } from "../../../store/actions/SonarrClientActions"
import { loadSonarrRootPaths as loadRootPaths } from "../../../store/actions/SonarrClientActions"
import { loadSonarrTags as loadTags } from "../../../store/actions/SonarrClientActions"
import { loadSonarrLanguages as loadLanguages } from "../../../store/actions/SonarrClientActions"
import { setSonarrCategory } from "../../../store/actions/SonarrClientActions"
import { removeSonarrCategory } from "../../../store/actions/SonarrClientActions"
import ValidatedTextbox from "../../Inputs/ValidatedTextbox"
import Dropdown from "../../Inputs/Dropdown"
import MultiDropdown from "../../Inputs/MultiDropdown"

import {
  FormGroup,
  Input,
  Row,
  Col,
  Collapse
} from "reactstrap";



function SonarrCategory(props) {
  const [nameErrorMessage, setNameErrorMessage] = useState("");
  const [isNameValid, setIsNameValid] = useState(true);
  const [isOpen, setIsOpen] = useState(false);

  const refPrevProps = useRef();

  const reduxState = useSelector((state) => {
    return {
      sonarr: state.tvShows.sonarr
    }
  });
  const dispatch = useDispatch();


  useEffect(() => {
    if (props.category.wasCreated)
      setIsOpen(true);
  }, []);


  useEffect(() => {
    const prevReduxState = refPrevProps.pastState;
    refPrevProps.pastState = reduxState;
    const prevProps = refPrevProps.pastProp;
    refPrevProps.pastProp = props;

    let previousNames = prevReduxState === undefined ? [] : prevReduxState.sonarr.categories.map(x => x.name);
    let currentNames = reduxState === undefined ? [] : reduxState.sonarr.categories.map(x => x.name);

    if (!(previousNames.length === currentNames.length && currentNames.every((value, index) => previousNames[index] === value)))
      validateName(props.category.name);

    if (props !== undefined && props.canConnect) {
      dispatch(loadProfiles(false));
      dispatch(loadRootPaths(false));
      dispatch(loadTags(false));
      dispatch(loadLanguages(false));
    }

    if ((prevProps !== undefined && prevProps.isSaving) !== (props !== undefined && props.isSaving))
      setIsOpen(false);
  });



  const validateName = (value) => {
    let newIsNameValid = true;
    let newNameErrorMessage = undefined;

    if (!/\S/.test(value)) {
      newNameErrorMessage = "A category name is required.";
      newIsNameValid = false;
    } else if (/^[\w-]{1,32}$/.test(value)) {
      if (reduxState.sonarr.categories.map(x => x.id).includes(props.category.id) && reduxState.sonarr.categories.filter(c => typeof c.id !== 'undefined' && c.id !== props.category.id && c.name.toLowerCase().trim() === value.toLowerCase().trim()).length > 0) {
        newNameErrorMessage = "All categories must have different names.";
        newIsNameValid = false;
      }
    } else {
      newNameErrorMessage = "Invalid categorie names, make sure they only contain alphanumeric characters, dashes and underscores. (No spaces, etc)";
      newIsNameValid = false;
    }

    setIsNameValid(newIsNameValid);
    if (newNameErrorMessage !== undefined)
      setNameErrorMessage(newNameErrorMessage);

    return newIsNameValid;
  };


  const setCategory = (fieldChanged, data) => {
    dispatch(setSonarrCategory(props.category.id, fieldChanged, data));
  };

  const deleteCategory = () => {
    setIsOpen(false);
    setTimeout(() => dispatch(removeSonarrCategory(props.category.id), 150));
  };





  return (
    <>
      <tr class="fade show">
        <th scope="row">
          <div class="media align-items-center">
            <div class="media-body">
              <span class="name mb-0 text-sm">{props.category.name}</span>
            </div>
          </div>
        </th>
        <td class="text-right">
          <button onClick={() => setIsOpen(!isOpen)} disabled={isOpen && (!isNameValid || !reduxState.sonarr.areLanguagesValid || !reduxState.sonarr.arePathsValid || !reduxState.sonarr.areProfilesValid || !reduxState.sonarr.areTagsValid)} className="btn btn-icon btn-3 btn-info" type="button">
            <span className="btn-inner--icon"><i class="fas fa-edit"></i></span>
            <span className="btn-inner--text">Edit</span>
          </button>
        </td>
      </tr>
      <tr>
        <td className="border-0 pb-0 pt-0" colSpan="2">
          <Collapse isOpen={isOpen}>
            <div className="mb-4">
              <Row>
                <Col lg="6">
                  <ValidatedTextbox
                    name="Category Name"
                    placeholder="Enter category name"
                    alertClassName="mt-3 text-wrap"
                    errorMessage={nameErrorMessage}
                    isSubmitted={props.isSubmitted || isNameValid}
                    value={props.category.name}
                    validation={validateName}
                    onChange={newName => setCategory("name", newName)}
                    onValidate={isValid => setIsNameValid(isValid)} />
                </Col>
              </Row>
              <Row>
                <Col lg="6" className="mb-4">
                  <div className="input-group-button">
                    <Dropdown
                      name="Path"
                      value={props.category.rootFolder}
                      items={reduxState.sonarr.paths.map(x => { return { name: x.path, value: x.path } })}
                      onChange={newPath => setCategory("rootFolder", newPath)} />
                    <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadRootPaths(true))} disabled={!props.canConnect} type="button">
                      <span className="btn-inner--icon">
                        {
                          reduxState.sonarr.isLoadingPaths ? (
                            <Oval
                              wrapperClass="loader"
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
                    !reduxState.sonarr.arePathsValid ? (
                      <Alert className="mt-3 mb-0 text-wrap " color="warning">
                        <strong>Could not find any paths.</strong>
                      </Alert>)
                      : null
                  }
                  {
                    props.isSubmitted && reduxState.sonarr.paths.length === 0 ? (
                      <Alert className="mt-3 mb-0 text-wrap " color="warning">
                        <strong>A path is required.</strong>
                      </Alert>)
                      : null
                  }
                </Col>
                <Col lg="6" className="mb-4">
                  <div className="input-group-button">
                    <Dropdown
                      name="Profile"
                      value={props.category.profileId}
                      items={reduxState.sonarr.profiles.map(x => { return { name: x.name, value: x.id } })}
                      onChange={newProfile => setCategory("profileId", newProfile)} />
                    <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadProfiles(true))} disabled={!props.canConnect} type="button">
                      <span className="btn-inner--icon">
                        {
                          reduxState.sonarr.isLoadingProfiles ? (
                            <Oval
                              wrapperClass="loader"
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
                    !reduxState.sonarr.areProfilesValid ? (
                      <Alert className="mt-3 mb-0 text-wrap " color="warning">
                        <strong>Could not find any profiles.</strong>
                      </Alert>)
                      : null
                  }
                  {
                    props.isSubmitted && reduxState.sonarr.profiles.length === 0 ? (
                      <Alert className="mt-3 mb-0 text-wrap " color="warning">
                        <strong>A profile is required.</strong>
                      </Alert>)
                      : null
                  }
                </Col>
              </Row>
              {
                props.apiVersion !== "2"
                  ? <>
                    <Row>
                      {props.apiVersion == "3"
                        ? <>
                          <Col lg="6">
                            <div className="input-group-button mb-4">
                              <Dropdown
                                name="Language"
                                value={props.category.languageId}
                                items={reduxState.sonarr.languages.map(x => { return { name: x.name, value: x.id } })}
                                onChange={newLanguage => setCategory("languageId", newLanguage)} />
                              <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadLanguages(true))} disabled={!props.canConnect} type="button">
                                <span className="btn-inner--icon">
                                  {
                                    reduxState.sonarr.isLoadingLanguages ? (
                                      <Oval
                                        wrapperClass="loader"
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
                              !reduxState.sonarr.areLanguagesValid ? (
                                <Alert className="mt-3 mb-4 text-wrap " color="warning">
                                  <strong>Could not find any languages.</strong>
                                </Alert>)
                                : null
                            }
                            {
                              props.isSubmitted && reduxState.sonarr.languages.length === 0 ? (
                                <Alert className="mt-3 mb-4 text-wrap " color="warning">
                                  <strong>A language is required.</strong>
                                </Alert>)
                                : null
                            }
                          </Col>
                        </>
                        : null}
                      <Col lg="6">
                        <div className="input-group-button mb-4">
                          <MultiDropdown
                            name="Tags"
                            placeholder=""
                            labelField="name"
                            valueField="id"
                            ignoreEmptyItems={true}
                            selectedItems={reduxState.sonarr.tags.filter(x => props.category.tags.includes(x.id))}
                            items={reduxState.sonarr.tags}
                            onChange={newTags => setCategory("tags", newTags.map(x => x.id))} />
                          <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadTags(true))} disabled={!props.canConnect} type="button">
                            <span className="btn-inner--icon">
                              {
                                reduxState.sonarr.isLoadingTags ? (
                                  <Oval
                                    wrapperClass="loader"
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
                          !reduxState.sonarr.areTagsValid ? (
                            <Alert className="mt-3 mb-4 text-wrap " color="warning">
                              <strong>Could not load tags, cannot reach Sonarr.</strong>
                            </Alert>)
                            : null
                        }
                      </Col>
                    </Row>
                  </>
                  : null
              }
              <Row>
                <Col lg="6">
                  <Dropdown
                    name="Series Type"
                    value={props.category.seriesType}
                    items={[{ name: "Standard", value: "standard" }, { name: "Daily", value: "daily" }, { name: "Anime", value: "anime" }, { name: "Automatically Detect", value: "automatic" }]}
                    onChange={newSeriesType => setCategory("seriesType", newSeriesType)} />
                </Col>
              </Row>
              <Row>
                <Col lg="6">
                  <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                    <Input
                      className="custom-control-input"
                      id={"tvUseSeasonFolders" + props.category.id}
                      type="checkbox"
                      onChange={() => setCategory("useSeasonFolders", !props.category.useSeasonFolders)}
                      checked={props.category.useSeasonFolders}
                    />
                    <label
                      className="custom-control-label"
                      htmlFor={"tvUseSeasonFolders" + props.category.id}>
                      <span className="text-muted">Use season folders</span>
                    </label>
                  </FormGroup>
                </Col>
              </Row>
              {
                reduxState.sonarr.categories.length > 1
                  ? <Row>
                    <Col lg="12" className="text-right">
                      <button onClick={() => deleteCategory()} className="btn btn-icon btn-3 btn-danger" type="button">
                        <span className="btn-inner--icon"><i class="fas fa-trash"></i></span>
                        <span className="btn-inner--text">Remove</span>
                      </button>
                    </Col>
                  </Row>
                  : null
              }
            </div>
          </Collapse>
        </td>
      </tr>
    </>
  );
}

export default SonarrCategory;