
import { useEffect, useRef, useState } from "react";
import { Oval } from 'react-loader-spinner'
import { useDispatch, useSelector } from 'react-redux';
import { Alert } from "reactstrap";
import { loadRadarrServiceSettings as loadServiceSettings } from "../../../../store/actions/OverseerrClientRadarrActions"
import { setOverseerrMovieCategory as setOverseerrCategory } from "../../../../store/actions/OverseerrClientRadarrActions"
import { removeOverseerrMovieCategory as removeOverseerrCategory } from "../../../../store/actions/OverseerrClientRadarrActions"
import ValidatedTextbox from "../../../Inputs/ValidatedTextbox"
import Dropdown from "../../../Inputs/Dropdown"
import MultiDropdown from "../../../Inputs/MultiDropdown"

import {
  FormGroup,
  Input,
  Row,
  Col,
  Collapse
} from "reactstrap";


function OverseerrMovieCategory(props) {
  const [nameErrorMessage, setNameErrorMessage] = useState("");
  const [isNameValid, setIsNameValid] = useState(true);
  const [isOpen, setIsOpen] = useState(false);

  const propsRef = useRef();

  const reduxState = useSelector((state) => {
    return {
      overseerr: state.movies.overseerr
    }
  });
  const dispatch = useDispatch();


  useEffect(() => {
    if (props.category.wasCreated)
      setIsOpen(true);

    if (props.canConnect)
      dispatch(loadServiceSettings(false));
  }, []);



  useEffect(() => {
    const prevState = propsRef?.pastState;
    propsRef.pastState = reduxState;
    const prevProps = propsRef?.past;
    propsRef.past = props;

    let previousNames = prevState === undefined ? [] : prevState.overseerr.categories.map(x => x.name);
    let currentNames = reduxState.overseerr.categories.map(x => x.name);

    if (props !== undefined && !(previousNames.length === currentNames.length && currentNames.every((value, index) => previousNames[index] === value))) {
      validateName(props.category.name)
    }

    if (props !== undefined && props?.canConnect) {
      dispatch(loadServiceSettings(false));
    }

    if (prevProps?.isSaving !== props?.isSaving && props !== undefined) {
      setIsOpen(false);
    }
  });


  // const validateNonEmptyString = (value) => {
  //   return /\S/.test(value);
  // };


  const validateName = (value) => {
    let newIsNameValid = true;
    let newNameErrorMessage = undefined;

    if (!/\S/.test(value)) {
      newNameErrorMessage = "A category name is required.";
      newIsNameValid = false;
    } else if (/^[\w-]{1,32}$/.test(value)) {
      if (reduxState.overseerr.categories.map(x => x.id).includes(props.category.id) && reduxState.overseerr.categories.filter(c => typeof c.id !== 'undefined' && c.id !== props.category.id && c.name.toLowerCase().trim() === value.toLowerCase().trim()).length > 0) {
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
    dispatch(setOverseerrCategory(props.category.id, fieldChanged, data));
  }

  const deleteCategory = () => {
    setIsOpen(false);
    setTimeout(() => dispatch(removeOverseerrCategory(props.category.id), 150));
  }

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
          <button onClick={() => setIsOpen(!isOpen)} disabled={isOpen && (!isNameValid || !reduxState.overseerr.isRadarrServiceSettingsValid)} className="btn btn-icon btn-3 btn-info" type="button">
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
                <Col lg="6" className="mb-4">
                  <div className="input-group-button">
                    <Dropdown
                      name="Radarr Instance"
                      value={props.category.serviceId}
                      items={reduxState.overseerr.radarrServiceSettings.radarrServices.map(x => { return { name: x.name, value: x.id } })}
                      onChange={newServiceId => setCategory("serviceId", newServiceId)} />
                    <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadServiceSettings(true))} disabled={!props.canConnect} type="button">
                      <span className="btn-inner--icon">
                        {
                          reduxState.overseerr.isLoadinRadarrServiceSettings ? (
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
                    reduxState.overseerr.radarrServiceSettings.radarrServices.length === 0 ? (
                      <Alert className="mt-3 mb-0 text-wrap " color="warning">
                        <strong>Could not find any radarr instances.</strong>
                      </Alert>)
                      : null
                  }
                </Col>
              </Row>
              <Row>
                <Col lg="6" className="mb-4">
                  <div className="input-group-button">
                    <Dropdown
                      name="Path"
                      value={props.category.rootFolder}
                      items={reduxState.overseerr.radarrServiceSettings.radarrServices.some(x => x.id === props.category.serviceId) ? reduxState.overseerr.radarrServiceSettings.radarrServices.filter(x => x.id === props.category.serviceId)[0].rootPaths.map(x => { return { name: x.name, value: x.name } }) : []}
                      onChange={newRootFolder => setCategory("rootFolder", newRootFolder)} />
                    <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadServiceSettings(true))} disabled={!props.canConnect} type="button">
                      <span className="btn-inner--icon">
                        {
                          reduxState.overseerr.isLoadinRadarrServiceSettings ? (
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
                    (reduxState.overseerr.radarrServiceSettings.radarrServices.some(x => x.id === props.category.serviceId) ? reduxState.overseerr.radarrServiceSettings.radarrServices.filter(x => x.id === props.category.serviceId)[0].rootPaths.map(x => { return { name: x.name, value: x.name } }) : []).length === 0 ? (
                      <Alert className="mt-3 mb-0 text-wrap " color="warning">
                        <strong>Could not find any paths.</strong>
                      </Alert>)
                      : null
                  }
                </Col>
                <Col lg="6" className="mb-4">
                  <div className="input-group-button">
                    <Dropdown
                      name="Profile"
                      value={props.category.profileId}
                      items={reduxState.overseerr.radarrServiceSettings.radarrServices.some(x => x.id === props.category.serviceId) ? reduxState.overseerr.radarrServiceSettings.radarrServices.filter(x => x.id === props.category.serviceId)[0].profiles.map(x => { return { name: x.name, value: x.id } }) : []}
                      onChange={newProfileId => setCategory("profileId", newProfileId)} />
                    <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadServiceSettings(true))} disabled={!props.canConnect} type="button">
                      <span className="btn-inner--icon">
                        {
                          reduxState.overseerr.isLoadinRadarrServiceSettings ? (
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
                    (reduxState.overseerr.radarrServiceSettings.radarrServices.some(x => x.id === props.category.serviceId) ? reduxState.overseerr.radarrServiceSettings.radarrServices.filter(x => x.id === props.category.serviceId)[0].profiles.map(x => { return { name: x.name, value: x.id } }) : []).length === 0 ? (
                      <Alert className="mt-3 mb-0 text-wrap " color="warning">
                        <strong>Could not find any profiles.</strong>
                      </Alert>)
                      : null
                  }
                </Col>
              </Row>
              <Row>
                <Col lg="6">
                  <div className="input-group-button mb-4">
                    <MultiDropdown
                      name="Tags"
                      placeholder=""
                      selectedItems={(reduxState.overseerr.radarrServiceSettings.radarrServices.some(x => x.id === props.category.serviceId) ? reduxState.overseerr.radarrServiceSettings.radarrServices.filter(x => x.id === props.category.serviceId)[0].tags : []).filter(x => props.category.tags.includes(x.id))}
                      items={reduxState.overseerr.radarrServiceSettings.radarrServices.some(x => x.id === props.category.serviceId) ? reduxState.overseerr.radarrServiceSettings.radarrServices.filter(x => x.id === props.category.serviceId)[0].tags : []}
                      onChange={newTags => setCategory("tags", newTags.map(x => x.id))} />
                    <button className="btn btn-icon btn-3 btn-default" onClick={() => dispatch(loadServiceSettings(true))} disabled={!props.canConnect} type="button">
                      <span className="btn-inner--icon">
                        {
                          reduxState.overseerr.isLoadinRadarrServiceSettings ? (
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
                    !reduxState.overseerr.isRadarrServiceSettingsValid ? (
                      <Alert className="mt-3 mb-4 text-wrap " color="warning">
                        <strong>Could not load tags, cannot reach Overseerr.</strong>
                      </Alert>)
                      : null
                  }
                </Col>
              </Row>
              <Row>
                <Col lg="6">
                  <FormGroup className="custom-control custom-control-alternative custom-checkbox mb-3">
                    <Input
                      className="custom-control-input"
                      id={"is4K" + props.category.id}
                      type="checkbox"
                      onChange={() => setCategory("is4K", !props.category.is4K)}
                      checked={props.category.is4K}
                    />
                    <label
                      className="custom-control-label"
                      htmlFor={"is4K" + props.category.id}>
                      <span className="text-muted">Is 4K Server</span>
                    </label>
                  </FormGroup>
                </Col>
              </Row>
              <Row>
                <Col lg="12" className="text-right">
                  <button onClick={() => deleteCategory()} className="btn btn-icon btn-3 btn-danger" type="button">
                    <span className="btn-inner--icon"><i class="fas fa-trash"></i></span>
                    <span className="btn-inner--text">Remove</span>
                  </button>
                </Col>
              </Row>
            </div>
          </Collapse>
        </td>
      </tr>
    </>
  );
}

export default OverseerrMovieCategory;