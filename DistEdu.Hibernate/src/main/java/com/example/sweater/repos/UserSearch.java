package com.example.sweater.repos;

import com.example.sweater.domain.User;
import org.hibernate.search.jpa.FullTextEntityManager;
import org.hibernate.search.query.dsl.QueryBuilder;
import org.springframework.stereotype.Repository;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.transaction.Transactional;
import java.util.List;

@Repository
@Transactional
public class UserSearch
{
  @PersistenceContext
  private EntityManager entityManager;

  public List<User> search(String text) {
    
    // get the full text entity manager
    FullTextEntityManager fullTextEntityManager =
      org.hibernate.search.jpa.Search.
      getFullTextEntityManager(entityManager);
    
    // create the query using Hibernate Search query DSL
    QueryBuilder queryBuilder = 
      fullTextEntityManager.getSearchFactory()
      .buildQueryBuilder().forEntity(User.class).get();
    
    // a very basic query by keywords
    org.apache.lucene.search.Query query =
      queryBuilder
        .keyword()
        .onFields("name", "city", "email")
        .matching(text)
        .createQuery();

    // wrap Lucene query in a Hibernate Query object
    org.hibernate.search.jpa.FullTextQuery jpaQuery =
      fullTextEntityManager.createFullTextQuery(query, User.class);
  
    // execute search and return results (sorted by relevance as default)
    @SuppressWarnings("unchecked")
    List<User> results = jpaQuery.getResultList();
    
    return results;
  }
}
