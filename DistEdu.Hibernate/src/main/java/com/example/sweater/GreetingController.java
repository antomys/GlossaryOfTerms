package com.example.sweater;

import com.example.sweater.domain.User;
import com.example.sweater.repos.UserRepo;
import com.example.sweater.repos.UserSearch;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;

import java.util.List;
import java.util.Map;

@Controller
public class GreetingController {

    @Autowired
    private UserSearch userSearch;

    @Autowired
    private UserRepo userRepo;

    @RequestMapping(value = "greeting", method = RequestMethod.GET)
    public String greeting(
            @RequestParam(name="name", required=false, defaultValue="World") String name,
            Map<String, Object> model
    )
    {
        model.put("name", name);
        return "greeting";
    }

    @RequestMapping(value = "/", method = RequestMethod.GET)
    public String main(Map<String, Object> model)
    {
        Iterable<User> messages = userRepo.findAll();

        model.put("messages", messages);

        return "main";
    }

    @RequestMapping(value = "add", method = RequestMethod.POST)
    public String add(@RequestParam String email, @RequestParam String name, @RequestParam String city, Map<String, Object> model)
    {
        User message = new User(email, name, city);

        userRepo.save(message);

        Iterable<User> messages = userRepo.findAll();

        model.put("messages", messages);

        return "main";
    }

    @RequestMapping(value = "filter", method = RequestMethod.POST)
    public String filter(@RequestParam String filter, Map<String, Object> model)
    {
        Iterable<User> searchResults = null;

        if (filter != null && !filter.isEmpty())
        {
            searchResults = userSearch.search(filter);
        }
        else
        {
            searchResults = userRepo.findAll();
        }

        model.put("messages", searchResults);
        return "main";
    }
}
