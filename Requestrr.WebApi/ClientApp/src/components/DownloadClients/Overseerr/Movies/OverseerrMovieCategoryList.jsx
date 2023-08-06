
import React from "react";
import { useDispatch, useSelector } from 'react-redux';
import { addOverseerrMovieCategory as addOverseerrCategory } from "../../../../store/actions/OverseerrClientRadarrActions"
import OverseerrMovieCategory from "./OverseerrMovieCategory";

// reactstrap components
import {
  FormGroup,
} from "reactstrap";


function OverseerrMovieCategoryList(props) {

  const reduxState = useSelector((state) => {
    return {
      overseerr: state.movies.overseerr
    }
  });
  const dispatch = useDispatch();


  const createOverseerrCategory = () => {
    let newId = Math.floor((Math.random() * 900) + 1);

    while (reduxState.overseerr.categories.map(x => x.id).includes(newId)) {
      newId = Math.floor((Math.random() * 900) + 1);
    }

    let newCategory = {
      id: newId,
      name: "new-category",
      serviceId: reduxState.overseerr.radarrServiceSettings.radarrServices.length > 0 ? reduxState.overseerr.radarrServiceSettings.radarrServices[0].id : -1,
      profileId: -1,
      rootFolder: "",
      tags: [],
      wasCreated: true
    };

    dispatch(addOverseerrCategory(newCategory));
  };


  return (
    <>
      <hr className="my-4" />
      <h6 className="heading-small text-muted">
        Overseerr Category Settings
      </h6>
      <div className="table-responsive mt-4 overflow-visible">
        <div>
          <table className="table align-items-center">
            <thead className="thead-dark">
              <tr>
                <th scope="col" className="sort" data-sort="name">Category</th>
                <th scope="col" className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="list">
              {reduxState.overseerr.categories.map((category, key) => {
                return (
                  <React.Fragment key={category.id}>
                    <OverseerrMovieCategory key={category.id} isSubmitted={props.isSubmitted} isSaving={props.isSaving} canConnect={props.canConnect} apiVersion={props.apiVersion} category={{ ...category }} />
                  </React.Fragment>)
              })}
              <tr>
                <td className="text-right" colSpan="2">
                  <FormGroup className="form-group text-right mt-2">
                    <button onClick={createOverseerrCategory} className="btn btn-icon btn-3 btn-success" type="button">
                      <span className="btn-inner--icon"><i className="fas fa-plus"></i></span>
                      <span className="btn-inner--text">Add new category</span>
                    </button>
                  </FormGroup>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <hr className="my-4" />
    </>
  );
}

export default OverseerrMovieCategoryList;