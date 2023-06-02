package com.microshop.microservice.product.controller;

import java.util.HashMap;
import java.util.Map;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class HomeController {
    @Value("${api.version}")
    private String apiVersion;

    @GetMapping
    @RequestMapping("/")
    public Map<String, String> getApiVersion() {
        var map = new HashMap<String, String>();
        map.put("api.version", apiVersion);
        return map;
    }
}
