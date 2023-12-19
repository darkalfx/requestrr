
import React from "react";
import { useDispatch, useSelector } from 'react-redux';
import { addSonarrCategory } from "../../../store/actions/SonarrClientActions"
import SonarrCategory from "./SonarrCategory";

// reactstrap components
import {
  FormGroup
} from "reactstrap";

function SonarrCategoryList(props) {

  const reduxState = useSelector((state) => {
    return {
      categories: state.tvShows.sonarr.categories
    }
  });
  const dispatch = useDispatch();

  const createSonarrCategory = () => {
    let newId = Math.floor((Math.random() * 900) + 1);

    while (reduxState.categories.map(x => x.id).includes(newId)) {
      newId = Math.floor((Math.random() * 900) + 1);
    }

    let newCategory = {
      id: newId,
      name: "new-category",
      profileId: -1,
      rootFolder: "",
      useSeasonFolders: true,
      seriesType: "standard",
      languageId: -1,
      tags: [],
      wasCreated: true
    };

    dispatch(addSonarrCategory(newCategory));
  };

  return (
    <>
      <hr className="my-4" />
      <h6 className="heading-small text-muted">
        Sonarr Category Settings
      </h6>
      <div class="table-responsive mt-4 overflow-visible">
        <div>
          <table class="table align-items-center">
            <thead class="thead-dark">
              <tr>
                <th scope="col" class="sort" data-sort="name">Category</th>
                <th scope="col" class="text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="list">
              {reduxState.categories.map((category, key) => {
                return (
                  <React.Fragment key={category.id}>
                    <SonarrCategory key={category.id} isSubmitted={props.isSubmitted} isSaving={props.isSaving} canConnect={props.canConnect} apiVersion={props.apiVersion} category={{ ...category }} />
                  </React.Fragment>)
              })}
              <tr>
                <td className="text-right" colSpan="2">
                  <FormGroup className="form-group text-right mt-2">
                    <button onClick={createSonarrCategory} className="btn btn-icon btn-3 btn-success" type="button">
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

export default SonarrCategoryList;