package com.example.sweater.domain;

import org.hibernate.search.annotations.Field;
import org.hibernate.search.annotations.Indexed;
import org.hibernate.search.annotations.Store;

import javax.persistence.*;
import javax.validation.constraints.NotNull;

@Entity
@Indexed
@Table(name = "users")
public class User {

  @Id
  @GeneratedValue(strategy = GenerationType.AUTO)
  private long id;

  @Field(store = Store.NO)
  @NotNull
  private String email;
  
  // store=Store.NO is the default values and could be omitted.
  @Field
  @NotNull
  private String name;

  @Field
  @NotNull
  private String city;

  public User() { }

  public User(long id) { 
    this.id = id;
  }

  public User(String email, String name) {
    this.email = email;
    this.name = name;
  }

  public User(String email, String name, String city) {
    this.email = email;
    this.name = name;
    this.city = city;
  }

  public long getId() {
    return id;
  }

  public void setId(long value) {
    this.id = value;
  }

  public String getEmail() {
    return email;
  }
  
  public void setEmail(String value) {
    this.email = value;
  }
  
  public String getName() {
    return name;
  }

  public void setName(String value) {
    this.name = value;
  }

  public String getCity() {
    return city;
  }

  public void setCity(String value) {
    this.city = value;
  }
}
